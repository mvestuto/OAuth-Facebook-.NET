using System;
using System.Data;
using gembrook.club.framework.classes;
using gembrook.club.framework.controls;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Web.SessionState;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace gembrook.club.basic_modules.club_admin.website {

	public class oAuthFacebook:BaseModuleControl { 
		public enum Method { GET, POST };
		public const string AUTHORIZE = "https://graph.facebook.com/oauth/authorize";
		public const string ACCESS_TOKEN = "https://graph.facebook.com/oauth/access_token";
		public const string CALLBACK_URL = "http://www.clubexpress.com/basic_modules/club_admin/website/auth_callback.aspx?type=facebook&state="; //must be www.clubexpress.com/
		public const string PAGE_TOKEN = "https://graph.facebook.com/me/accounts?access_token"; // final get request for page access token
		private string _consumerKey = "551830351604240";
		private string _consumerSecret = "06fad3607496cdd966627ea09218e94b";
		private string _token = "";
		private string _longToken = "";
		private string _pageToken = "";

		public string ConsumerKey {
			get {
				if (_consumerKey.Length == 0) {
					_consumerKey = "551830351604240"; 
				}
				return _consumerKey;
			}
			set { _consumerKey = value; }
		}

		public string ConsumerSecret {
			get {
				if (_consumerSecret.Length == 0) {
					_consumerSecret = "06fad3607496cdd966627ea09218e94b"; 
				}
				return _consumerSecret;
			}
			set { _consumerSecret = value; }
		}

		public string Token { get { return _token; } set { _token = value; } }
		public string longToken { get { return _longToken; } set { _longToken = value; } }

		//AuthorizationLinkGet
		public string AuthorizationLinkGet(string cacheKey) {
			string constructedUrl = CALLBACK_URL + cacheKey;
			return string.Format("{0}?client_id={1}&redirect_uri={2}&scope=manage_pages,publish_actions,publish_pages", AUTHORIZE, this.ConsumerKey, constructedUrl);
		}

		//AccessTokenGet - Get and Exchange Facebook Access Token
		public void AccessTokenGet(string authToken, string cacheKey) {
			this.Token = authToken;
			string constructedUrl = CALLBACK_URL + cacheKey;
			string accessTokenUrl = string.Format("{0}?client_id={1}&redirect_uri={2}&client_secret={3}&code={4}",
			ACCESS_TOKEN, this.ConsumerKey, constructedUrl, this.ConsumerSecret, authToken);

			string response = WebRequest(Method.GET, accessTokenUrl, String.Empty);

			if (response.Length > 0) {
				//Store the returned access_token
				NameValueCollection qs = HttpUtility.ParseQueryString(response,System.Text.Encoding.UTF8);

				string decoded = HttpUtility.UrlDecode(response);
				JToken token = JObject.Parse(decoded);
				string access_token = (string)token.SelectToken("access_token");

				if (access_token != null) {
					this.Token = access_token;
					exchangeShortLived(this.Token);
				}
			}
		}
		public void exchangeShortLived(string shortToken) {
			string accessTokenUrl = string.Format("{0}?grant_type=fb_exchange_token&client_id={1}&client_secret={2}&fb_exchange_token={3}",
			ACCESS_TOKEN, this.ConsumerKey, this.ConsumerSecret, shortToken);

			string response = WebRequest(Method.GET, accessTokenUrl, String.Empty);

			if (response.Length > 0) {
				//Store the returned access_token
				NameValueCollection qs = HttpUtility.ParseQueryString(response);
				string decoded = HttpUtility.UrlDecode(response);
				JToken token = JObject.Parse(decoded);
				string access_token = (string)token.SelectToken("access_token");

				if (access_token != null) {
					this.Token = access_token; //now we have the long term token
				}
			}
		}

		//WebRequest - ovverides System.Net WebRequest
		public string WebRequest(Method method, string url, string postData) {

			System.Net.HttpWebRequest webRequest = null;
			StreamWriter requestWriter = null;
			string responseData = "";

			webRequest = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
			webRequest.Method = method.ToString();
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.UserAgent = HttpContext.Current.Request.UserAgent;
			webRequest.Timeout = 20000;

			if (method == Method.POST) {
				webRequest.ContentType = "application/x-www-form-urlencoded";

				//POST the data.
				requestWriter = new StreamWriter(webRequest.GetRequestStream());

				try {
					requestWriter.Write(postData);
				}
				catch {
					throw;
				}

				finally {
					requestWriter.Close();
					requestWriter = null;
				}
			}

			responseData = WebResponseGet(webRequest);
			webRequest = null;
			return responseData;
		}

		//WebRequest - ovverides System.Net WebRequest
		public static string PageTokenRequest(string url, string postData) {

			System.Net.HttpWebRequest webRequest = null;
			string responseData = "";

			webRequest = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.UserAgent = HttpContext.Current.Request.UserAgent;
			webRequest.Timeout = 20000;

			responseData = WebResponseGet(webRequest);
			webRequest = null;
			return responseData;
		}

		//WebResponseGet
		public static string WebResponseGet(System.Net.HttpWebRequest webRequest) {
			StreamReader responseReader = null;
			string responseData = "";
			HttpContext context = HttpContext.Current;
			HttpSessionState session = context.Session;
			try {
				responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
				responseData = responseReader.ReadToEnd();
			}
			catch {
				throw;
			}
			finally {
				webRequest.GetResponse().GetResponseStream().Close();
				responseReader.Close();
				responseReader = null;
			}

			return responseData;
		}
	}
}
