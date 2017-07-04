/***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************/
"use strict"

function CmisObjectModelConnector() {
    this.currentApplicationDomain = '';

    this.embeddedFrame = null;
    this.embeddedFrameUri = 'b1dd0f15138a41d5994a2df00b7f2816';

    this.initialized = false;
    
    this.loginCallback = null;
    this.logoutCallback = null;
    this.loginKey = ' ';
    this.loginKeyCookie = 'a4b2726d8a59435b85527a833cfe194c';

    this.repositoryDomain = '6352c757d11f45d7b7408a2e468ce249';
    this.serviceUri = '5f2bd79dc36843c6a0794545f9c5350d';
    this.sessionIdCookie = '63d92532245a4c8998c095ac30d33690';

    this.tokenCallbacks = new Array();
}

/*
When calling this method first it initializes the embedded frame to allow access to repositoryDomain.
Following requests will lead direct to a login request.
*/
CmisObjectModelConnector.prototype.cmisLogin = function (callback) {
    //wait for previous login request
    if (this.loginCallback != null)
        callback(false);
    else {
        this.loginCallback = callback;
        if (!this.initialized) {
            //initialize() will call postLoginMessage()
            this.initialize();
        }
        else
            this.postLoginMessage();
    }
}

/*
Logout from repository
*/
CmisObjectModelConnector.prototype.cmisLogout = function (callback) {
    //wait for previous logout request or not logged in
    if ((this.logoutCallback != null) || !this.initialized)
        callback(false);
    else {
        var message = 'logout:cmisaction=logout';
        if (this.loginKey != '')
            message = message + '&' + this.loginKey;
        this.logoutCallback = callback;
        this.embeddedFrame.contentWindow.postMessage(message, this.repositoryDomain);
    }
}

/*
CmisObjectModelLibrary-servers accept all client-generated tokens
*/
CmisObjectModelConnector.prototype.cmisNextToken = function (callback) {
    //login first
    if (!this.initialized) {
        //signal invalid state
        callback('');
    }
    else {
        //trigger next token
        this.tokenCallbacks.unshift(callback);
        this.iframe.contentWindow.postMessage('token:', this.repositoryDomain);
    }
}

CmisObjectModelConnector.prototype.cmisServiceURL = function () {
    return this.serviceUri;
}

/*
Event-listener for application window
*/
CmisObjectModelConnector.prototype.eventListener = function (e) {
    if (e.origin != this.repositoryDomain) {
        //listen only for messages from the embedded frame
        return;
    }
    if (e.data.substring(0, 6) == 'token:') {
        if (this.tokenCallbacks.length > 0) {
            // at least one callback is waiting for a token

            // create a valid token
            var token = this.findCookie(this.sessionIdCookie, '') + '\r\n' + this.newGuid();

            // trigger callback
            this.tokenCallbacks.pop()(token);
        }
    }
    else if (e.data.substring(0, 9) == 'redirect:') {
        //silent login was not successful
        var stringifiedParameters = e.data.substring(9);
        var parameters = JSON.parse(stringifiedParameters);
        var loginUri = parameters.loginUri;

        this.loginCallback = null;
        this.loginKey = parameters.loginKey;
        this.setCookie(document, this.loginKeyCookie, this.loginKey, 3600);

        window.location.href = loginUri;
    }
    else if (e.data.substring(0, 9) == 'loggedIn:') {
        //silent login was successful
        var callback = this.loginCallback;

        this.loginKey = e.data.substring(9);
        if (callback != null) {
            this.loginCallback = null;
            callback(true);
        }

    }
    else if ((e.data.substring(0, 10) == 'loggedOut:') && (this.logoutCallback != null)) {
        //logged out from repository
        var success = ('ok' == e.data.substring(10));
        var callback = this.logoutCallback;

        this.logoutCallback = null;
        callback(success);
    }
}

/*
Find cookie in the document.cookie. If document.cookie doesn't contain the searched cookie, the fallbackValue is returned
*/
CmisObjectModelConnector.prototype.findCookie = function (cookie, fallbackValue) {
    var ca = document.cookie.split(';');
    //search for requested cookie
    for (var i = 0; i < ca.length; i++) {
        //Trim()
        var c = this.trim(ca[i]);
        if (c.indexOf(cookie + '=') == 0) {
            return unescape(c.substring(cookie.length + 1));
        }
    }

    //not found
    return fallbackValue;
}

/*
Initialization
*/
CmisObjectModelConnector.prototype.initialize = function () {
    var self = this;

    var loginKey = this.trim(this.findCookie(this.loginKeyCookie, this.loginKey));
    if (loginKey != this.loginKey) {
        //remove cookie
        this.setCookie(document, this.loginKeyCookie, '', 0);
        this.loginKey = loginKey;
    }
    //application info
    this.currentApplicationDomain = window.location.protocol + '//' + window.location.host;
    var encodedApplicationDomain = encodeURIComponent(this.currentApplicationDomain);
    var encodedApplicationPath = encodeURIComponent(window.location.pathname + window.location.search);

    //install listener for application main window
    window.addEventListener('message', function (e) { self.eventListener(e) }, false);

    //embed frame
    var embeddedFrame = document.createElement('embeddedFrame');
    embeddedFrame.src = this.embeddedFrameUri + (this.embeddedFrameUri.indexOf('?') == -1 ? '?' : '&') + 'domain=' + encodedApplicationDomain + '&path=' + encodedApplicationPath;
    embeddedFrame.style.display = 'none';
    embeddedFrame.onload = function () { self.postLoginMessage() }
    this.embeddedFrame = document.getElementsByTagName('body').item(0).appendChild(embeddedFrame);

    //initialization done
    this.initialized = true;
}

/*
Create random string in guid format
*/
CmisObjectModelConnector.prototype.newGuid = (function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
               .toString(16)
               .substring(1);
    }
    return function () {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
           s4() + '-' + s4() + s4() + s4();
    };
})();

/*
Send login request to the embedded frame
*/
CmisObjectModelConnector.prototype.postLoginMessage = function () {
    //the request must be done in the repository-domain
    var message = 'login:cmisaction=login&accepts=' + encodeURIComponent('application/json');
    if (this.loginKey != '')
        message = message + '&' + this.loginKey;
    this.embeddedFrame.contentWindow.postMessage(message, this.repositoryDomain);
}

/*
Sets or removes a cookie
*/
CmisObjectModelConnector.prototype.setCookie = function (document, cookie, value, maxAge) {
    if (maxAge === 0) {
        var expires = new Date();
        expires.setTime(expires.getTime() - 3600);
        cookie = cookie + '=_; Max-Age=0; expires=' + expires.toGMTString();
    }
    else {
        cookie = cookie + "='" + escape(value) + "'";
        if (maxAge > 0) {
            var expires = new Date();
            expires.setTime(expires.getTime() + maxAge);
            cookie = cookie + '; Max-Age=' + maxAge + ';expires=' + expires.toGMTString();
        }
    }
    document.cookie = cookie;
}

CmisObjectModelConnector.prototype.trim = function (input) {
    return input.replace(/^\s\s*/, '').replace(/\s\s*$/, '');
}

var cmisObjectModelConnector = new CmisObjectModelConnector();


/*******************************************************************************************************
'* Functions defined by the CMIS 1.1 specification
'* see http://docs.oasis-open.org/cmis/CMIS/v1.1/os/CMIS-v1.1-os.html
'*     chapter 5.2.9.2.2 Login and Tokens
*******************************************************************************************************/
function cmisServiceURL() {
    return cmisObjectModelConnector.cmisServiceURL();
}

function cmisLogin(callback) {
    cmisObjectModelConnector.cmisLogin(callback);
}

function cmisLogout(callback) {
    cmisObjectModelConnector.cmisLogout(callback);
}

function cmisNextToken(callback) {
    cmisObjectModelConnector.cmisNextToken(callback);
}