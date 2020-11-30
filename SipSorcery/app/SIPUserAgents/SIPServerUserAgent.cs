//-----------------------------------------------------------------------------
// Filename: SIPServerUserAgent.cs
//
// Description: Implementation of a SIP Server User Agent that can be used to receive SIP calls.
//
// Author(s):
// Aaron Clauson (aaron@sipsorcery.com)
//
// History:
// 22 Feb 2008	Aaron Clauson   Created, Hobart, Australia.
// rj2: added overloads for Answer/Reject/Redirect-methods with/out customHeader
//
// License: 
// BSD 3-Clause "New" or "Revised" License, see included LICENSE.md file.
//-----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SIPSorcery.Net;
using SIPSorcery.Sys;

namespace SIPSorcery.SIP.App
{
    /// <summary>
    /// Implementation of a SIP Server User Agent that can be used to receive SIP calls.
    /// </summary>
    public class SIPServerUserAgent : ISIPServerUserAgent
    {
        private static ILogger logger = Log.Logger;

        private SIPAuthenticateRequestDelegate SIPAuthenticateRequest_External;
        private GetSIPAccountDelegate GetSIPAccount_External;

        private SIPTransport m_sipTransport;
        private UASInviteTransaction m_uasTransaction;
        private SIPEndPoint m_outboundProxy;                   // If the system needs to use an outbound proxy for every request this will be set and overrides any user supplied values.
        private bool m_isAuthenticated;
        private bool m_isCancelled;
        private bool m_isHungup;
        private string m_owner;
        private string m_adminMemberId;
        private string m_sipUsername;
        private string m_sipDomain;
        private SIPDialogueTransferModesEnum m_transferMode;

        public bool IsB2B { get { return false; } }
        public bool IsInvite
        {
            get { return true; }
        }
        public string Owner { get { return m_owner; } }

        /// <summary>
        /// Call direction for this user agent.
        /// </summary>
        public SIPCallDirection CallDirection { get; private set; } = SIPCallDirection.In;

        /// <summary>
        /// The SIP dialog that's created if we're able to successfully answer the call request.
        /// </summary>
        public SIPDialogue SIPDialogue { get; private set; }

        private SIPAccount m_sipAccount;
        public SIPAccount SIPAccount
        {
            get { return m_sipAccount; }
            set { m_sipAccount = value; }
        }

        public bool IsAuthenticated
        {
            get { return m_isAuthenticated; }
            set { m_isAuthenticated = value; }
        }

        public bool IsCancelled
        {
            get { return m_isCancelled; }
        }

        public SIPRequest CallRequest
        {
            get { return m_uasTransaction.TransactionRequest; }
        }

        public string CallDestination
        {
            get { return m_uasTransaction.TransactionRequest.URI.User; }
        }

        public bool IsUASAnswered
        {
            get { return m_uasTransaction != null && m_uasTransaction.TransactionFinalResponse != null; }
        }

        public bool IsHungup
        {
            get { return m_isHungup; }
        }

        public UASInviteTransaction ClientTransaction
        {
            get { return m_uasTransaction; }
        }

        /// <summary>
        /// The Session Description Protocol offer from the remote call party.
        /// </summary>
        public SDP OfferSDP
        {
            get
            {
                if (!String.IsNullOrEmpty(m_uasTransaction.TransactionRequest.Body))
                {
                    return SDP.ParseSDPDescription(m_uasTransaction.TransactionRequest.Body);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The caller cancelled the call request.
        /// </summary>
        public event SIPUASDelegate CallCancelled;

        /// <summary>
        /// This end of the call timed out providing a ringing response. This situation can occur for SIP servers.
        /// They will attempt to forward the call to a SIP account's contacts. If none reply then the will never
        /// continue past the trying stage.
        /// </summary>
        public event SIPUASDelegate NoRingTimeout;

        /// <summary>
        /// The underlying invite transaction has reached the completed state.
        /// </summary>
        public event SIPUASDelegate TransactionComplete;

        /// <summary>
        /// The underlying invite transaction has changed state.
        /// </summary>
        public event SIPUASStateChangedDelegate UASStateChanged;

        /// <summary>
        /// Gets fired when the call successfully negotiates an SDP offer/answer and creates a new dialog.
        /// Typically this can occur at the same time as the transaction final response is sent. But in cases
        /// where the initial INVITE does not contain an SDP offer the dialog will not be created until the 
        /// ACK is received.
        /// </summary>
        public event Action<SIPDialogue> OnDialogueCreated;

        public SIPServerUserAgent(
            SIPTransport sipTransport,
            SIPEndPoint outboundProxy,
            string sipUsername,
            string sipDomain,
            SIPCallDirection callDirection,
            GetSIPAccountDelegate getSIPAccount,
            SIPAuthenticateRequestDelegate sipAuthenticateRequest,
            UASInviteTransaction uasTransaction)
        {
            m_sipTransport = sipTransport;
            m_outboundProxy = outboundProxy;
            m_sipUsername = sipUsername;
            m_sipDomain = sipDomain;
            CallDirection = callDirection;
            GetSIPAccount_External = getSIPAccount;
            SIPAuthenticateRequest_External = sipAuthenticateRequest;
            m_uasTransaction = uasTransaction;

            m_uasTransaction.UASInviteTransactionTimedOut += ClientTimedOut;
            m_uasTransaction.UASInviteTransactionCancelled += UASTransactionCancelled;
            m_uasTransaction.TransactionRemoved += new SIPTransactionRemovedDelegate(UASTransaction_TransactionRemoved);
        }

        private void UASTransaction_TransactionRemoved(SIPTransaction sipTransaction)
        {
            TransactionComplete?.Invoke(this);
        }

        public bool LoadSIPAccountForIncomingCall()
        {
            try
            {
                bool loaded = false;

                if (GetSIPAccount_External == null)
                {
                    // No point trying to authenticate if we haven't been given a delegate to load the SIP account.
                    Reject(SIPResponseStatusCodesEnum.InternalServerError, null, null);
                }
                else
                {
                    m_sipAccount = GetSIPAccount_External(m_sipUsername, m_sipDomain);

                    if (m_sipAccount == null)
                    {
                        // A full lookup failed. Now try a partial lookup if the incoming username is in a dotted domain name format.
                        if (m_sipUsername.Contains("."))
                        {
                            string sipUsernameSuffix = m_sipUsername.Substring(m_sipUsername.LastIndexOf(".") + 1);
                            m_sipAccount = GetSIPAccount_External(sipUsernameSuffix, m_sipDomain);
                        }

                        if (m_sipAccount == null)
                        {
                            logger.LogWarning("Rejecting public call for " + m_sipUsername + "@" + m_sipDomain + ", SIP account not found.");
                            Reject(SIPResponseStatusCodesEnum.NotFound, null, null);
                        }
                        else
                        {
                            loaded = true;
                        }
                    }
                    else
                    {
                        loaded = true;
                    }
                }

                if (loaded)
                {
                    SetOwner(m_sipAccount.Owner, m_sipAccount.AdminMemberId);
                }

                return loaded;
            }
            catch (Exception excp)
            {
                logger.LogError("Exception LoadSIPAccountForIncomingCall. " + excp.Message);
                Reject(SIPResponseStatusCodesEnum.InternalServerError, null, null);
                return false;
            }
        }

        public bool AuthenticateCall()
        {
            m_isAuthenticated = false;

            try
            {
                if (SIPAuthenticateRequest_External == null)
                {
                    // No point trying to authenticate if we haven't been given an authentication delegate.
                    Reject(SIPResponseStatusCodesEnum.InternalServerError, null, null);
                }
                else if (GetSIPAccount_External == null)
                {
                    // No point trying to authenticate if we haven't been given a  delegate to load the SIP account.
                    Reject(SIPResponseStatusCodesEnum.InternalServerError, null, null);
                }
                else
                {
                    m_sipAccount = GetSIPAccount_External(m_sipUsername, m_sipDomain);

                    if (m_sipAccount == null)
                    {
                        logger.LogWarning("Rejecting authentication required call for " + m_sipUsername + "@" + m_sipDomain + ", SIP account not found.");
                        Reject(SIPResponseStatusCodesEnum.Forbidden, null, null);
                    }
                    else
                    {
                        SIPRequest sipRequest = m_uasTransaction.TransactionRequest;
                        SIPEndPoint localSIPEndPoint = (!sipRequest.Header.ProxyReceivedOn.IsNullOrBlank()) ? SIPEndPoint.ParseSIPEndPoint(sipRequest.Header.ProxyReceivedOn) : sipRequest.LocalSIPEndPoint;
                        SIPEndPoint remoteEndPoint = (!sipRequest.Header.ProxyReceivedFrom.IsNullOrBlank()) ? SIPEndPoint.ParseSIPEndPoint(sipRequest.Header.ProxyReceivedFrom) : sipRequest.RemoteSIPEndPoint;

                        SIPRequestAuthenticationResult authenticationResult = SIPAuthenticateRequest_External(localSIPEndPoint, remoteEndPoint, sipRequest, m_sipAccount);
                        if (authenticationResult.Authenticated)
                        {
                            if (authenticationResult.WasAuthenticatedByIP)
                            {
                                logger.LogDebug("New call from " + remoteEndPoint.ToString() + " successfully authenticated by IP address.");
                            }
                            else
                            {
                                logger.LogDebug("New call from " + remoteEndPoint.ToString() + " successfully authenticated by digest.");
                            }

                            SetOwner(m_sipAccount.Owner, m_sipAccount.AdminMemberId);
                            m_isAuthenticated = true;
                        }
                        else
                        {
                            // Send authorisation failure or required response
                            SIPResponse authReqdResponse = SIPResponse.GetResponse(sipRequest, authenticationResult.ErrorResponse, null);
                            authReqdResponse.Header.AuthenticationHeader = authenticationResult.AuthenticationRequiredHeader;
                            logger.LogWarning("Call not authenticated for " + m_sipUsername + "@" + m_sipDomain + ", responding with " + authenticationResult.ErrorResponse + ".");
                            m_uasTransaction.SendFinalResponse(authReqdResponse);
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                logger.LogError("Exception SIPServerUserAgent AuthenticateCall. " + excp.Message);
                Reject(SIPResponseStatusCodesEnum.InternalServerError, null, null);
            }

            return m_isAuthenticated;
        }

        public void Progress(SIPResponseStatusCodesEnum progressStatus, string reasonPhrase, string[] customHeaders, string progressContentType, string progressBody)
        {
            try
            {
                if (!IsUASAnswered)
                {
                    if ((int)progressStatus >= 200)
                    {
                        logger.LogDebug("UAS call was passed an invalid response status of " + (int)progressStatus + ", ignoring.");
                    }
                    else
                    {
                        UASStateChanged?.Invoke(this, progressStatus, reasonPhrase);

                        // Allow all Trying responses through as some may contain additional useful information on the call state for the caller. 
                        // Also if the response is a 183 Session Progress with audio forward it.
                        if (m_uasTransaction.TransactionState == SIPTransactionStatesEnum.Proceeding && progressStatus != SIPResponseStatusCodesEnum.Trying &&
                            !(progressStatus == SIPResponseStatusCodesEnum.SessionProgress && progressBody != null))
                        {
                            logger.LogDebug("UAS call ignoring progress response with status of " + (int)progressStatus + " as already in " + m_uasTransaction.TransactionState + ".");
                        }
                        else
                        {
                            logger.LogDebug("UAS call progressing with " + progressStatus + ".");
                            SIPResponse progressResponse = SIPResponse.GetResponse(m_uasTransaction.TransactionRequest, progressStatus, reasonPhrase);

                            if (progressResponse.Status != SIPResponseStatusCodesEnum.Trying)
                            {
                                progressResponse.Header.To.ToTag = m_uasTransaction.LocalTag;
                            }

                            if (!progressBody.IsNullOrBlank())
                            {
                                progressResponse.Body = progressBody;
                                progressResponse.Header.ContentType = progressContentType;
                                progressResponse.Header.ContentLength = progressBody.Length;
                            }

                            if (customHeaders != null && customHeaders.Length > 0)
                            {
                                foreach (string header in customHeaders)
                                {
                                    progressResponse.Header.UnknownHeaders.Add(header);
                                }
                            }

                            m_uasTransaction.SendProvisionalResponse(progressResponse);
                        }
                    }
                }
                else
                {
                    logger.LogWarning("SIPServerUserAgent Progress fired on already answered call.");
                }
            }
            catch (Exception excp)
            {
                logger.LogError("Exception SIPServerUserAgent Progress. " + excp.Message);
            }
        }

        public SIPDialogue Answer(string contentType, string body, SIPDialogueTransferModesEnum transferMode)
        {
            return Answer(contentType, body, null, transferMode);
        }

        public SIPDialogue Answer(string contentType, string body, SIPDialogueTransferModesEnum transferMode, string[] customHeaders)
        {
            return Answer(contentType, body, null, transferMode, customHeaders);
        }

        public SIPDialogue Answer(string contentType, string body, string toTag, SIPDialogueTransferModesEnum transferMode)
        {
            return Answer(contentType, body, toTag, transferMode, null);
        }

        public SIPDialogue Answer(string contentType, string body, string toTag, SIPDialogueTransferModesEnum transferMode, string[] customHeaders)
        {
            try
            {
                m_transferMode = transferMode;

                if (m_uasTransaction.TransactionFinalResponse != null)
                {
                    logger.LogDebug("UAS Answer was called on an already answered call, ignoring.");
                    return null;
                }
                else
                {
                    UASStateChanged?.Invoke(this, SIPResponseStatusCodesEnum.Ok, null);

                    if (!toTag.IsNullOrBlank())
                    {
                        m_uasTransaction.LocalTag = toTag;
                    }

                    SIPResponse okResponse = m_uasTransaction.GetOkResponse(contentType, body);

                    if (body != null)
                    {
                        okResponse.Header.ContentType = contentType;
                        okResponse.Header.ContentLength = body.Length;
                        okResponse.Body = body;
                    }
                    if (customHeaders != null && customHeaders.Length > 0)
                    {
                        foreach (string header in customHeaders)
                        {
                            okResponse.Header.UnknownHeaders.Add(header);
                        }
                    }

                    if (OfferSDP == null)
                    {
                        // The INVITE request did not contain an SDP offer. We need to send the offer in the response and
                        // then get the answer from the ACK.
                        m_uasTransaction.OnAckReceived += OnAckAnswerReceived;
                    }

                    m_uasTransaction.SendFinalResponse(okResponse);

                    if (OfferSDP != null)
                    {
                        SIPDialogue = new SIPDialogue(m_uasTransaction);
                        SIPDialogue.TransferMode = transferMode;

                        OnDialogueCreated?.Invoke(SIPDialogue);

                        return SIPDialogue;
                    }
                    else
                    {
                        // The dialogue cannot be created until the ACK is received.
                        return null;
                    }
                }
            }
            catch (Exception excp)
            {
                logger.LogError("Exception SIPServerUserAgent Answer. " + excp.Message);
                throw;
            }
        }

        private Task<SocketError> OnAckAnswerReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction sipTransaction, SIPRequest sipRequest)
        {
            SIPDialogue = new SIPDialogue(m_uasTransaction);
            SIPDialogue.TransferMode = m_transferMode;

            OnDialogueCreated?.Invoke(SIPDialogue);

            return Task.FromResult(SocketError.Success);
        }

        public void AnswerNonInvite(SIPResponseStatusCodesEnum answerStatus, string reasonPhrase, string[] customHeaders, string contentType, string body)
        {
            throw new NotImplementedException();
        }

        public void Reject(SIPResponseStatusCodesEnum failureStatus, string reasonPhrase)
        {
            Reject(failureStatus, reasonPhrase, null);
        }

        public void Reject(SIPResponseStatusCodesEnum failureStatus, string reasonPhrase, string[] customHeaders)
        {
            try
            {
                if (m_uasTransaction.TransactionFinalResponse == null)
                {
                    if ((int)failureStatus < 400)
                    {
                        logger.LogDebug("UAS Reject was passed an invalid response status of " + (int)failureStatus + ", ignoring.");
                    }
                    else
                    {
                        UASStateChanged?.Invoke(this, failureStatus, reasonPhrase);

                        string failureReason = (!reasonPhrase.IsNullOrBlank()) ? " and " + reasonPhrase : null;

                        logger.LogWarning("UAS call failed with a response status of " + (int)failureStatus + failureReason + ".");
                        SIPResponse failureResponse = SIPResponse.GetResponse(m_uasTransaction.TransactionRequest, failureStatus, reasonPhrase);

                        if (customHeaders != null && customHeaders.Length > 0)
                        {
                            foreach (string header in customHeaders)
                            {
                                failureResponse.Header.UnknownHeaders.Add(header);
                            }
                        }

                        m_uasTransaction.SendFinalResponse(failureResponse);
                    }
                }
                else
                {
                    logger.LogWarning("SIPServerUserAgent Reject fired on already answered call.");
                }
            }
            catch (Exception excp)
            {
                logger.LogError("Exception SIPServerUserAgent Reject. " + excp.Message);
            }
        }

        public void Redirect(SIPResponseStatusCodesEnum redirectCode, SIPURI redirectURI)
        {
            Redirect(redirectCode, redirectURI, null);
        }
        public void Redirect(SIPResponseStatusCodesEnum redirectCode, SIPURI redirectURI, string[] customHeaders)
        {
            try
            {
                if (m_uasTransaction.TransactionFinalResponse == null)
                {
                    SIPResponse redirectResponse = SIPResponse.GetResponse(m_uasTransaction.TransactionRequest, redirectCode, null);
                    redirectResponse.Header.Contact = SIPContactHeader.CreateSIPContactList(redirectURI);

                    if (customHeaders != null && customHeaders.Length > 0)
                    {
                        foreach (string header in customHeaders)
                        {
                            redirectResponse.Header.UnknownHeaders.Add(header);
                        }
                    }

                    m_uasTransaction.SendFinalResponse(redirectResponse);
                }
            }
            catch (Exception excp)
            {
                logger.LogError("Exception SIPServerUserAgent Redirect. " + excp.Message);
            }
        }

        public void NoCDR()
        {
            m_uasTransaction.CDR = null;
        }

        /// <summary>
        /// Used to hangup the call or indicate that the client hungup.
        /// </summary>
        /// <param name="clientHungup">True if the BYE request was received from the client. False if the hangup
        /// needs to originate from this agent.</param>
        public void Hangup(bool clientHungup)
        {
            if (!m_isHungup)
            {
                m_isHungup = true;

                if (SIPDialogue == null)
                {
                    return;
                }

                // Only need to send a BYE request if the client didn't already do so.
                if (clientHungup == false)
                {
                    try
                    {
                        // Cases found where the Contact in the INVITE was to a different protocol than the original request.
                        var inviteContact = m_uasTransaction.TransactionRequest.Header.Contact.FirstOrDefault();
                        if (inviteContact == null)
                        {
                            logger.LogWarning("The Contact header on the INVITE request was missing, BYE request cannot be generated.");
                        }
                        else
                        {
                            //SIPRequest byeRequest = GetByeRequest();
                            var byeRequest = SIPDialogue.GetInDialogRequest(SIPMethodsEnum.BYE);
                            SIPNonInviteTransaction byeTransaction = new SIPNonInviteTransaction(m_sipTransport, byeRequest, m_outboundProxy);
                            byeTransaction.NonInviteTransactionFinalResponseReceived += ByeServerFinalResponseReceived;
                            byeTransaction.SendRequest();
                        }
                    }
                    catch (Exception excp)
                    {
                        logger.LogError("Exception SIPServerUserAgent Hangup. " + excp.Message);
                        throw;
                    }
                }
            }
        }

        private Task<SocketError> ByeServerFinalResponseReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction sipTransaction, SIPResponse sipResponse)
        {
            try
            {
                logger.LogDebug("Response " + sipResponse.StatusCode + " " + sipResponse.ReasonPhrase + " for " + sipTransaction.TransactionRequest.URI.ToString() + ".");

                SIPNonInviteTransaction byeTransaction = sipTransaction as SIPNonInviteTransaction;

                if ((sipResponse.Status == SIPResponseStatusCodesEnum.ProxyAuthenticationRequired || sipResponse.Status == SIPResponseStatusCodesEnum.Unauthorised) && SIPAccount != null)
                {
                    // Resend BYE with credentials.
                    SIPAuthorisationDigest authRequest = sipResponse.Header.AuthenticationHeader.SIPDigest;
                    SIPURI contactUri = sipResponse.Header.Contact.Any() ? sipResponse.Header.Contact[0].ContactURI : sipResponse.Header.From.FromURI;

                    authRequest.SetCredentials(SIPAccount.SIPUsername, SIPAccount.SIPPassword, contactUri.ToString(), SIPMethodsEnum.BYE.ToString());

                    SIPRequest authByeRequest = byeTransaction.TransactionRequest;
                    authByeRequest.Header.AuthenticationHeader = new SIPAuthenticationHeader(authRequest);
                    authByeRequest.Header.AuthenticationHeader.SIPDigest.Response = authRequest.Digest;
                    authByeRequest.Header.Vias.TopViaHeader.Branch = CallProperties.CreateBranchId();
                    authByeRequest.Header.CSeq = authByeRequest.Header.CSeq + 1;

                    SIPNonInviteTransaction authByeTransaction = new SIPNonInviteTransaction(m_sipTransport, authByeRequest, null);
                    authByeTransaction.SendRequest();
                }

                return Task.FromResult(SocketError.Success);
            }
            catch (Exception excp)
            {
                logger.LogError("Exception ByServerFinalResponseReceived. " + excp.Message);
                return Task.FromResult(SocketError.Fault);
            }
        }

        private void UASTransactionCancelled(SIPTransaction sipTransaction)
        {
            logger.LogDebug("SIPServerUserAgent got cancellation request.");
            m_isCancelled = true;
            CallCancelled?.Invoke(this);
        }

        private void ClientTimedOut(SIPTransaction sipTransaction)
        {
            try
            {
                logger.LogDebug($"SIPServerUserAgent client timed out in transaction state {m_uasTransaction.TransactionState}.");

                logger.LogWarning("UAS for " + m_uasTransaction.TransactionRequest.URI.ToString() + " timed out in transaction state " + m_uasTransaction.TransactionState + ".");

                //if (m_uasTransaction.TransactionState == SIPTransactionStatesEnum.Calling && NoRingTimeout != null)
                //{
                NoRingTimeout?.Invoke(this);
                //}
            }
            catch (Exception excp)
            {
                logger.LogError("Exception ClientTimedOut. " + excp.Message);
            }
        }

        public void SetOwner(string owner, string adminMemberId)
        {
            m_owner = owner;
            m_adminMemberId = adminMemberId;

            if (m_uasTransaction.CDR != null)
            {
                m_uasTransaction.CDR.Owner = owner;
                m_uasTransaction.CDR.AdminMemberId = adminMemberId;

                m_uasTransaction.CDR.Updated();
            }
        }

        public void SetDialPlanContextID(Guid dialPlanContextID)
        {
            if (m_uasTransaction.CDR != null)
            {
                m_uasTransaction.CDR.DialPlanContextID = dialPlanContextID;
                m_uasTransaction.CDR.Updated();
            }
        }

        //private SIPRequest GetByeRequest()
        //{
        //    SIPRequest byeRequest = new SIPRequest(SIPMethodsEnum.BYE, SIPDialogue.RemoteTarget);
        //    SIPFromHeader byeFromHeader = SIPFromHeader.ParseFromHeader(SIPDialogue.LocalUserField.ToString());
        //    SIPToHeader byeToHeader = SIPToHeader.ParseToHeader(SIPDialogue.RemoteUserField.ToString());
        //    int cseq = SIPDialogue.CSeq + 1;

        //    SIPHeader byeHeader = new SIPHeader(byeFromHeader, byeToHeader, cseq, SIPDialogue.CallId);
        //    byeHeader.CSeqMethod = SIPMethodsEnum.BYE;
        //    byeRequest.Header = byeHeader;
        //    byeRequest.Header.Routes = SIPDialogue.RouteSet;
        //    byeRequest.Header.ProxySendFrom = SIPDialogue.ProxySendFrom;
        //    byeRequest.Header.Vias.PushViaHeader(SIPViaHeader.GetDefaultSIPViaHeader());

        //    return byeRequest;
        //}
    }
}
