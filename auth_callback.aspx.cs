using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using gembrook.club.framework.classes;
using gembrook.club.framework.controls;

namespace gembrook.club.basic_modules.club_admin.website {
	public class auth_callback: BasePage {
		public SessionData _sessionData;
		protected string _callType;
		protected string _tempToken;
		protected string _twitterToken;
		protected string _facebookToken;
		protected string _linkedInToken;
		protected string _redirectString;
		protected string _twitterVerifier;
		protected string _twitterSecret;
		protected string _tempLinkedInToken;
		protected string _keyString;
		protected string _receivedClubId;
		protected string _receivedCacheKeyPrefix;
		protected string _receivedNonce; //for use in reconstructing system cache keys
		protected string _linkedInCacheKeyPrefix = "LinkedInKey";
		protected string _twitterSecretKeyPrefix = "TwitterSecretKey";
		protected string _twitterSecretKey;
		protected string _clubDomain;
		protected string _tokenToBeCached;
		oAuthFacebook oFace = new oAuthFacebook();
		oAuthTwitter oTwitter = new oAuthTwitter();
		oAuthLinkedIn oLinkedIn = new oAuthLinkedIn();

		private void Page_Load(object sender, EventArgs e) {

			string url = "";
			_callType = Request.QueryString["type"];

			if (_callType == "facebook") {
				//Get the access token.
				_keyString = Request.QueryString["state"];
				_receivedClubId = _keyString.Split('-')[2];
				_receivedCacheKeyPrefix = _keyString.Split('-')[0];
				oFace.AccessTokenGet(Request["code"], _keyString);
				if (oFace.Token.Length > 0) {
					_facebookToken = oFace.Token;
					_tokenToBeCached = oFace.Token;
					//We now have the short lived token, need to get long lived token

				}
			}

			
			if (_callType == "twitter") {
				_keyString = Request.QueryString["state"];
				_receivedClubId = _keyString.Split('-')[2];
				_receivedCacheKeyPrefix = _keyString.Split('-')[0];
				_receivedNonce = _keyString.Split('-')[1];
				_twitterSecretKey = _twitterSecretKeyPrefix + "-" + _receivedNonce + "-" + _receivedClubId;  //Twitter Secret must be cached so it can be received by social_networking_push
																											//uses same random nonce that was provided by caller - now caller knows key
				_tempToken = Request.QueryString["oauth_token"];
				_tempToken = Request["oauth_token"].ToString();

				//Get the access token and secret.
				_twitterVerifier = Request["oauth_verifier"].ToString();
				oTwitter.AccessTokenGet(_tempToken, _twitterVerifier);
				if (oTwitter.Token.Length > 0) {
					_twitterToken = oTwitter.Token;
					_twitterSecret = oTwitter.TokenSecret;
					_tokenToBeCached = oTwitter.Token;
					//We now have the credentials, so we can start making API calls
				}
			}
			//LinkedIn does not support additional Query parameters, so prefix/key had to be stored in "state" parameter. If we hit this page without a type, it's linkedin.
			if (string.IsNullOrEmpty(_callType)) {
				_keyString = Request["state"];
				if (!String.IsNullOrEmpty(_keyString)) {
					_receivedClubId = _keyString.Split('-')[2];
					_receivedCacheKeyPrefix = _keyString.Split('-')[0];
				}
			}
			
			if (_receivedCacheKeyPrefix == _linkedInCacheKeyPrefix) {
				string linkedinCode = Request["code"];
				if (!String.IsNullOrEmpty(linkedinCode)) {
					oLinkedIn.AccessTokenGet(linkedinCode);
					if (oLinkedIn.Token.Length > 0) {
						_linkedInToken = oLinkedIn.Token;
						_tokenToBeCached = oLinkedIn.Token;
						//We now have the credentials, so we can start making API calls
					}
				}
			}
			
			cacheTokens();
			}
		// saveTokens
		private void cacheTokens() {
			if (!string.IsNullOrEmpty(_tokenToBeCached)) {
				CacheManager.addItemToCache(_keyString, _receivedClubId, _tokenToBeCached, 5);
			}
			if (_callType == "twitter") { //have to also cache the twitter secret
				CacheManager.addItemToCache(_twitterSecretKey, _receivedClubId, _twitterSecret, 5);
			}
		}
		
	}

}