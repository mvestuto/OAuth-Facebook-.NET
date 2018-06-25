using System;
using System.Collections.Generic;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace gembrook.club.basic_modules.club_admin.website{
	public class oAuthBase2 {
		public enum SignatureTypes {
			HMACSHA1,
			PLAINTEXT,
			RSASHA1
		}

		/// <summary>
		/// Provides an internal structure to sort the query parameter
		/// </summary>
		protected class QueryParameter {
			private string name = null;
			private string value = null;

			public QueryParameter(string name, string value) {
				this.name = name;
				this.value = value;
			}

			public string Name {
				get { return name; }
			}

			public string Value {
				get { return value; }
			}
		}

		//QueryParameterComparer 
		protected class QueryParameterComparer : IComparer<QueryParameter> {

			#region IComparer<QueryParameter> Members

			public int Compare(QueryParameter x, QueryParameter y) {
				if (x.Name == y.Name) {
					return string.Compare(x.Value, y.Value);
				}
				else {
					return string.Compare(x.Name, y.Name);
				}
			}

			#endregion
		}

		protected const string OAuthVersion = "1.0";
		protected const string OAuthParameterPrefix = "oauth_";

		//
		// List of know and used oauth parameters' names
		//        
		protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
		protected const string OAuthCallbackKey = "oauth_callback";
		protected const string OAuthVersionKey = "oauth_version";
		protected const string OAuthSignatureMethodKey = "oauth_signature_method";
		protected const string OAuthSignatureKey = "oauth_signature";
		protected const string OAuthTimestampKey = "oauth_timestamp";
		protected const string OAuthNonceKey = "oauth_nonce";
		protected const string OAuthTokenKey = "oauth_token";
		protected const string OAuthVerifierKey = "oauth_verifier";
		protected const string oAauthVerifier = "oauth_verifier";
		protected const string OAuthTokenSecretKey = "oauth_token_secret";
		protected const string EntitiesKey = "include_entities";
		protected const string HMACSHA1SignatureType = "HMAC-SHA1";
		protected const string PlainTextSignatureType = "PLAINTEXT";
		protected const string RSASHA1SignatureType = "RSA-SHA1";

		protected Random random = new Random();

		private string oauth_verifier;
		public string Verifier { get { return oauth_verifier; } set { oauth_verifier = value; } }


		protected string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

		//ComputeHash
		private string ComputeHash(HashAlgorithm hashAlgorithm, string data) {
			if (hashAlgorithm == null) {
				throw new ArgumentNullException("hashAlgorithm");
			}

			if (string.IsNullOrEmpty(data)) {
				throw new ArgumentNullException("data");
			}

			byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
			byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

			return Convert.ToBase64String(hashBytes);
		}

		//GetQueryParameters
		private List<QueryParameter> GetQueryParameters(string parameters) {
			if (parameters.StartsWith("?")) {
				parameters = parameters.Remove(0, 1);
			}

			List<QueryParameter> result = new List<QueryParameter>();

			if (!string.IsNullOrEmpty(parameters)) {
				string[] p = parameters.Split('&');
				foreach (string s in p) {
					if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix)) {
						if (s.IndexOf('=') > -1) {
							string[] temp = s.Split('=');
							result.Add(new QueryParameter(temp[0], temp[1]));
						}
						else {
							result.Add(new QueryParameter(s, string.Empty));
						}
					}
				}
			}

			return result;
		}

		//UrlEncode
		public string UrlEncode(string value) {
			StringBuilder result = new StringBuilder();

			foreach (char symbol in value) {
				if (unreservedChars.IndexOf(symbol) != -1) {
					result.Append(symbol);
				}
				else {
					result.Append('%' + String.Format("{0:X2}", (int)symbol));
				}
			}

			return result.ToString();
		}

		//NormalizeRequestParameters - Normalizes paramters per spec
		protected string NormalizeRequestParameters(IList<QueryParameter> parameters) {
			StringBuilder sb = new StringBuilder();
			QueryParameter p = null;
			for (int i = 0; i < parameters.Count; i++) {
				p = parameters[i];
				sb.AppendFormat("{0}={1}", p.Name, p.Value);

				if (i < parameters.Count - 1) {
					sb.Append("&");
				}
			}

			return sb.ToString();
		}

		//GenerateSignatureBase - Used in the AuthorizationGet
		public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters) {
			if (token == null) {
				token = string.Empty;
			}

			if (tokenSecret == null) {
				tokenSecret = string.Empty;
			}

			if (string.IsNullOrEmpty(consumerKey)) {
				throw new ArgumentNullException("consumerKey");
			}

			if (string.IsNullOrEmpty(httpMethod)) {
				throw new ArgumentNullException("httpMethod");
			}

			if (string.IsNullOrEmpty(signatureType)) {
				throw new ArgumentNullException("signatureType");
			}

			normalizedUrl = null;
			normalizedRequestParameters = null;

			List<QueryParameter> parameters = GetQueryParameters(url.Query);
			parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
			parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
			parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
			parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
			parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

			if (!string.IsNullOrEmpty(token)) {
				parameters.Add(new QueryParameter(OAuthTokenKey, token));
			}

			if (!string.IsNullOrEmpty(oauth_verifier)) {
				parameters.Add(new QueryParameter(oAauthVerifier, oauth_verifier));
			}


			parameters.Sort(new QueryParameterComparer());


			normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
			if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443))) {
				normalizedUrl += ":" + url.Port;
			}
			normalizedUrl += url.AbsolutePath;
			normalizedRequestParameters = NormalizeRequestParameters(parameters);

			StringBuilder signatureBase = new StringBuilder();
			signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
			signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
			signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

			return signatureBase.ToString();
		}


		//GenerateTwitterSignatureBase
		public string GenerateTwitterSignatureBase(Uri url, string callback, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string verifier, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters, int leg) {
			if (callback == null) {
				callback = string.Empty;
			}
			if (token == null) {
				token = string.Empty;
			}

			if (tokenSecret == null) {
				tokenSecret = string.Empty;
			}

			if (verifier == null) {
				verifier = string.Empty;
			}

			if (string.IsNullOrEmpty(consumerKey)) {
				throw new ArgumentNullException("consumerKey");
			}

			if (string.IsNullOrEmpty(httpMethod)) {
				throw new ArgumentNullException("httpMethod");
			}

			if (string.IsNullOrEmpty(signatureType)) {
				throw new ArgumentNullException("signatureType");
			}

			normalizedUrl = null;
			normalizedRequestParameters = null;

			List<QueryParameter> parameters = GetQueryParameters(url.Query);
			parameters.Add(new QueryParameter(EntitiesKey, "true"));
			parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));
			parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
			parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
			parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
			if (!string.IsNullOrEmpty(token)) {
				parameters.Add(new QueryParameter(OAuthTokenKey, token));
			}
			parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));

			if (!string.IsNullOrEmpty(callback) && leg != 3) {
				parameters.Add(new QueryParameter(OAuthCallbackKey, UrlEncode(callback)));
			}

			if (!string.IsNullOrEmpty(verifier) && leg != 3) {
				parameters.Add(new QueryParameter(OAuthVerifierKey, verifier));
			}

			parameters.Sort(new QueryParameterComparer());

			normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
			if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443))) {
				normalizedUrl += ":" + url.Port;
			}
			normalizedUrl += url.AbsolutePath;
			normalizedRequestParameters = NormalizeRequestParameters(parameters);

			StringBuilder signatureBase = new StringBuilder();
			signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
			signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
			signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

			return signatureBase.ToString();
        }


		//GenerateSignatureUsingHash - Encodes Signature
		public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash) {
			return ComputeHash(hash, signatureBase);
		}

		//GenerateTwitterSignature
		public string GenerateTwitterSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string callBackUrl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters, int leg) {
			normalizedUrl = null;
			normalizedRequestParameters = null;

            switch (signatureType) {
                case SignatureTypes.PLAINTEXT:					
                    return UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMACSHA1:					
					string signatureBase = GenerateTwitterSignatureBase(url, callBackUrl, consumerKey, token, tokenSecret, httpMethod, timeStamp, oauthVerifier, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters, leg);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);                                        
                case SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
			}
		}

		public string GenerateTwitterSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string callBackUrl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters, int leg) {
			return GenerateTwitterSignature(url, consumerKey, consumerSecret, token, tokenSecret, callBackUrl, oauthVerifier, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters, leg);
		}

		//GenerateSignature (Twitter Uses a special signature - see above_)
		public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters) {
			normalizedUrl = null;
			normalizedRequestParameters = null;

			switch (signatureType) {
				case SignatureTypes.PLAINTEXT:
					return HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
				case SignatureTypes.HMACSHA1:
					string signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, httpMethod, timeStamp, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters);

					HMACSHA1 hmacsha1 = new HMACSHA1();
					hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));

					return GenerateSignatureUsingHash(signatureBase, hmacsha1);
				case SignatureTypes.RSASHA1:
					throw new NotImplementedException();
				default:
					throw new ArgumentException("Unknown signature type", "signatureType");
			}
		}

		public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters) {
			return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
		}
		//GenerateTimeStamp
		public virtual string GenerateTimeStamp() {
			// Default implementation of UNIX time of the current UTC time
			TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return Convert.ToInt64(ts.TotalSeconds).ToString();
		}

		//Generate Nonce
		public virtual string GenerateNonce() {
			//returns a secure nonce per oAuth2 spec
			return Guid.NewGuid().ToString().Replace("-", "");
		}
		//Generate Random String 
		public virtual string GenerateRandomNumber() {
			// Just a simple implementation of a random number between 123400 and 9999999
			//used in cache Key generation 
			return random.Next(123400, 9999999).ToString();
		}
	}
}
