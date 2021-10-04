'***********************************************************************************************************************
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
'***********************************************************************************************************************
Imports CmisObjectModel.Common
Imports CmisObjectModel.Constants
Imports sn = System.Net
Imports sri = System.Runtime.InteropServices
Imports ssc = System.Security.Cryptography
Imports ssd = System.ServiceModel.Description
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client
   Public Class AuthenticationProvider
      Inherits Common.AuthenticationInfo

      Public Sub New(user As String, password As System.Security.SecureString)
         MyBase.New(user, password)
         _cookies = New sn.CookieContainer()
      End Sub

      ''' <summary>
      ''' Creates a secure NtlmAuthenticationProvider if the password only contains digits and letters.
      ''' Otherwise it creates a AuthenticationProvider to prevent problems with special characters.
      ''' </summary>
      ''' <param name="user"></param>
      ''' <param name="password"></param>
      ''' <returns></returns>
      Public Shared Shadows Function CreateInstance(user As String, password As System.Security.SecureString) As AuthenticationProvider
         For Each ch As Char In PasswordToCharArray(password)
            If Not Char.IsLetterOrDigit(ch) Then Return New AuthenticationProvider(user, password)
         Next
         Return New NtlmAuthenticationProvider(user, password)
      End Function

#Region "Authentication"
      Public Sub Authenticate(portOrRequest As Object)
         If TypeOf portOrRequest Is sn.HttpWebRequest Then
            HttpAuthenticate(CType(portOrRequest, sn.HttpWebRequest))
         Else
            WebServiceAuthenticate(portOrRequest)
         End If
      End Sub

      ''' <summary>
      ''' Authentication AtomPub-Binding
      ''' </summary>
      ''' <param name="request"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub HttpAuthenticate(request As sn.HttpWebRequest)
         request.AllowWriteStreamBuffering = False
         request.CookieContainer = _cookies
         If request.Headers.GetValues("Authorization") Is Nothing AndAlso Not (String.IsNullOrEmpty(_user) AndAlso (_password Is Nothing OrElse _password.Length = 0)) Then
            Dim chars As New List(Of Char)
            Dim valuePtr As IntPtr = IntPtr.Zero

            If Not String.IsNullOrEmpty(_user) Then chars.AddRange(_user.ToCharArray())
            chars.Add(":"c)

            If _password IsNot Nothing AndAlso _password.Length > 0 Then
               Try
                  valuePtr = sri.Marshal.SecureStringToGlobalAllocUnicode(_password)
                  For index As Integer = 0 To _password.Length - 1
                     chars.Add(ChrW(sri.Marshal.ReadInt16(valuePtr, index << 1)))
                  Next
               Finally
                  sri.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr)
               End Try
            End If
            request.Headers.Add("Authorization", "Basic " & System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(chars.ToArray())))
         End If
      End Sub

      ''' <summary>
      ''' Authentication WebService-Binding
      ''' </summary>
      ''' <param name="port"></param>
      ''' <remarks>Not supported</remarks>
      Protected Sub WebServiceAuthenticate(port As Object)
      End Sub

      ''' <summary>
      ''' Authentication WebService-Binding
      ''' </summary>
      ''' <param name="endPoint"></param>
      ''' <param name="clientCredentials"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub AddWebServiceCredentials(endPoint As ssd.ServiceEndpoint, clientCredentials As ssd.ClientCredentials)
         If String.IsNullOrEmpty(_user) AndAlso (_password Is Nothing OrElse _password.Length = 0) Then
            Dim binding As System.ServiceModel.Channels.CustomBinding = TryCast(endPoint.Binding, System.ServiceModel.Channels.CustomBinding)
            'remove SecurityBindingElement because neither a username nor a password have been set
            If binding IsNot Nothing Then binding.Elements.RemoveAll(Of System.ServiceModel.Channels.SecurityBindingElement)()
         Else
            Dim valuePtr As IntPtr = IntPtr.Zero

            clientCredentials.UserName.UserName = If(_user, "")
            If _password IsNot Nothing AndAlso _password.Length > 0 Then
               Try
                  valuePtr = sri.Marshal.SecureStringToGlobalAllocUnicode(_password)
                  clientCredentials.UserName.Password = sri.Marshal.PtrToStringUni(valuePtr)
               Finally
                  sri.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr)
               End Try
            End If
         End If
      End Sub
#End Region

      Protected _caseInsensitiveCookies As New Collections.Generic.DictionaryTree(Of String, sn.Cookie)()
      Public ReadOnly Property CaseInsensitiveCookies(uri As Uri) As sn.CookieCollection
         Get
            Dim retVal As New sn.CookieCollection()
            Dim host As String = uri.Host
            Dim hostLowerInvariant As String = host.ToLowerInvariant()

            If _caseInsensitiveCookies.ContainsKeys(hostLowerInvariant) Then
               Dim domainTrees As Dictionary(Of String, Collections.Generic.DictionaryTree(Of String, sn.Cookie)) = _caseInsensitiveCookies.Tree(hostLowerInvariant).SubTrees
               Dim key As String = String.Empty
               Dim absolutePath As String = uri.AbsolutePath
               Dim regEx As New System.Text.RegularExpressions.Regex("(\/|[^\/]*)")
               Dim existingCookies As Dictionary(Of String, sn.Cookie) = (From item As Object In _cookies.GetCookies(uri)
                                                                          Let cookie As sn.Cookie = TryCast(item, sn.Cookie)
                                                                          Where cookie IsNot Nothing
                                                                          Select cookie).ToDictionary(Function(current) current.Name)
               For Each match As System.Text.RegularExpressions.Match In regEx.Matches(absolutePath.ToLowerInvariant())
                  key &= match.Value.ToLowerInvariant()
                  If domainTrees.ContainsKey(key) Then
                     For Each de As KeyValuePair(Of String, Collections.Generic.DictionaryTree(Of String, sn.Cookie)) In domainTrees(key).SubTrees
                        If Not existingCookies.ContainsKey(de.Key) Then
                           With de.Value.Item
                              retVal.Add(New sn.Cookie(.Name, .Value, absolutePath.Substring(0, key.Length), host))
                           End With
                        End If
                     Next
                  End If
               Next
            End If

            Return retVal
         End Get
      End Property
      Public WriteOnly Property CaseInsensitiveCookies As sn.CookieCollection
         Set(value As sn.CookieCollection)
            For Each cookie As sn.Cookie In value
               _caseInsensitiveCookies.Item(cookie.Domain.ToLowerInvariant, cookie.Path.ToLowerInvariant(), cookie.Name) = cookie
            Next
         End Set
      End Property

      Protected _cookies As sn.CookieContainer
      Public Property Cookies As sn.CookieContainer
         Get
            Return _cookies
         End Get
         Set(value As sn.CookieContainer)
            _cookies = value
         End Set
      End Property

   End Class
End Namespace