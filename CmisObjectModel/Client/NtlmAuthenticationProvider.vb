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
   Public Class NtlmAuthenticationProvider
      Inherits AuthenticationProvider

      Public Sub New(user As String, password As System.Security.SecureString)
         MyBase.New(user, password)
      End Sub

#Region "Authentication"
      ''' <summary>
      ''' Authentication AtomPub-Binding
      ''' </summary>
      ''' <param name="request"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub HttpAuthenticate(request As System.Net.HttpWebRequest)
         If String.IsNullOrEmpty(_user) AndAlso (_password Is Nothing OrElse _password.Length = 0) Then
            request.Credentials = sn.CredentialCache.DefaultNetworkCredentials
         Else
            request.Credentials = New sn.NetworkCredential(_user, _password)
         End If
         request.CookieContainer = _cookies
         request.AllowWriteStreamBuffering = True
      End Sub

      ''' <summary>
      ''' Authentication WebService-Binding
      ''' </summary>
      ''' <param name="endPoint"></param>
      ''' <param name="clientCredentials"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub AddWebServiceCredentials(endPoint As ssd.ServiceEndpoint, clientCredentials As ssd.ClientCredentials)
         Dim binding As System.ServiceModel.Channels.CustomBinding = TryCast(endPoint.Binding, System.ServiceModel.Channels.CustomBinding)

         If binding IsNot Nothing Then
            'remove SecurityBindingElement (reset before setting the credentials)
            binding.Elements.RemoveAll(Of System.ServiceModel.Channels.SecurityBindingElement)()
            If String.IsNullOrEmpty(_user) AndAlso (_password Is Nothing OrElse _password.Length = 0) Then
               clientCredentials.Windows.ClientCredential = sn.CredentialCache.DefaultNetworkCredentials
            Else
               clientCredentials.Windows.ClientCredential = New sn.NetworkCredential(_user, _password)
            End If
         End If
      End Sub
#End Region

   End Class
End Namespace