using System;

namespace agsXMPP.protocol.sasl
{
    public enum FailureCondition
    {
        
        /// <summary>
        /// The receiving entity acknowledges an <abort/> element sent by the initiating entity; sent in reply to the <abort/> element.
        /// </summary>
        aborted,
        
        /// <summary>
        /// The data provided by the initiating entity could not be processed because the [BASE64] (Josefsson, S., “The Base16, Base32, and Base64 Data Encodings,” July 2003.) encoding is incorrect (e.g., because the encoding does not adhere to the definition in Section 3 of [BASE64] (Josefsson, S., “The Base16, Base32, and Base64 Data Encodings,” July 2003.)); sent in reply to a <response/> element or an <auth/> element with initial response data.
        /// </summary>
        incorrect_encoding,
        
        /// <summary>
        /// The authzid provided by the initiating entity is invalid, either because it is incorrectly formatted or because the initiating entity does not have permissions to authorize that ID; sent in reply to a <response/> element or an <auth/> element with initial response data.
        /// </summary>
        invalid_authzid,

        /// <summary>
        /// The initiating entity did not provide a mechanism or requested a mechanism that is not supported by the receiving entity; sent in reply to an <auth/> element.
        /// </summary>
        invalid_mechanism,
        
        /// <summary>
        /// The mechanism requested by the initiating entity is weaker than server policy permits for that initiating entity; sent in reply to a <response/> element or an <auth/> element with initial response data.
        /// </summary>
        mechanism_too_weak,
        
        /// <summary>
        /// The authentication failed because the initiating entity did not provide valid credentials (this includes but is not limited to the case of an unknown username); sent in reply to a <response/> element or an <auth/> element with initial response data.
        /// </summary>
        not_authorized,
        
        /// <summary>
        /// The authentication failed because of a temporary error condition within the receiving entity; sent in reply to an <auth/> element or <response/> element.
        /// </summary>
        temporary_auth_failure,

        UnknownCondition
    }
}
