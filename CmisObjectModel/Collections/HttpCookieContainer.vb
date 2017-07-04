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
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Collections
   Public Class HttpCookieContainer

      Public Sub New(headers As System.Net.WebHeaderCollection, header As String)
         Dim regEx As New System.Text.RegularExpressions.Regex("(?<Name>[^,\s=]+)\s*=\s*" & GetStringPattern("Value") &
                                                               "(\s*;\s*((?<CookieAV>Expires)\s*=\s*(?<Expires>[\s\S]+GMT)" &
                                                                       "|(?<CookieAV>Max\-Age)\s*=\s*(?<MaxAge>\d+)" &
                                                                       "|(?<CookieAV>Domain)\s*=\s*" & GetStringPattern("Domain") &
                                                                       "|(?<CookieAV>Path)\s*=\s*" & GetStringPattern("Path") &
                                                                       "|(?<CookieAV>(Secure|HttpOnly))" &
                                                                       "|(?<CookieAV_Name>[^\s,;]+)\s*=\s*" & GetStringPattern("CookieAV_Value") &
                                                                       "|" & GetStringPattern("CookieAV") & "))*",
                                                               Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                                               Text.RegularExpressions.RegexOptions.IgnoreCase Or
                                                               Text.RegularExpressions.RegexOptions.Singleline)
         _headers = headers
         _header = header

         If Not (_headers Is Nothing OrElse String.IsNullOrEmpty(header)) Then
            Dim cookies As String = headers(header)
            If Not String.IsNullOrEmpty(cookies) Then
               For Each match As System.Text.RegularExpressions.Match In regEx.Matches(cookies)
                  Dim cookie As New HttpCookie(match.Groups("Name").Value, TryUnescapeDataString(If(match.Groups("Value").Value, String.Empty)))
                  Dim group As System.Text.RegularExpressions.Group = match.Groups("CookieAV")

                  If group IsNot Nothing AndAlso group.Success Then
                     For Each capture As System.Text.RegularExpressions.Capture In group.Captures
                        Select Case If(capture.Value, String.Empty).ToLowerInvariant()
                           Case "domain"
                              cookie.Domain = match.Groups("Domain").Value
                           Case "expires"
                              Dim expires As DateTime
                              If DateTime.TryParse(match.Groups("Expires").Value, expires) Then cookie.Expires = expires
                           Case "httponly"
                              cookie.HttpOnly = True
                           Case "max-age"
                              Dim maxAge As Integer
                              If Integer.TryParse(match.Groups("MaxAge").Value, maxAge) Then cookie.MaxAge = maxAge
                           Case "path"
                              cookie.Path = match.Groups("Path").Value
                           Case "secure"
                              cookie.Secure = True
                           Case Else
                              cookie.AddExtension(capture.Value, Nothing)
                        End Select
                     Next
                  End If

                  group = match.Groups("CookieAV_Name")
                  If group IsNot Nothing AndAlso group.Success Then
                     Dim extensionKeys As System.Text.RegularExpressions.CaptureCollection = group.Captures
                     Dim extensionValues As System.Text.RegularExpressions.CaptureCollection = match.Groups("CookieAV_Value").Captures

                     For index As Integer = 0 To extensionKeys.Count - 1
                        cookie.AddExtension(extensionKeys(index).Value, TryUnescapeDataString(extensionValues(index).Value))
                     Next
                  End If
                  AddOrReplaceCore(cookie)
               Next
            End If
         End If
      End Sub

      ''' <summary>
      ''' Adds a cookie, if cookie.Name does not exists in the container. Otherwise cookie replaces the existing cookie.
      ''' </summary>
      ''' <param name="cookie"></param>
      ''' <remarks></remarks>
      Public Sub AddOrReplace(cookie As HttpCookie)
         If AddOrReplaceCore(cookie) Then Refresh()
      End Sub
      Private Function AddOrReplaceCore(cookie As HttpCookie) As Boolean
         Dim name As String = If(cookie Is Nothing, Nothing, cookie.Name)

         If Not String.IsNullOrEmpty(name) Then
            If _cookies.ContainsKey(name) Then
               _cookies(name).Owner = Nothing
               _cookies.Remove(name)
            End If
            _cookies.Add(name, cookie)
            cookie.Owner = Me

            Return True
         Else
            Return False
         End If
      End Function

      ''' <summary>
      ''' Returns the cookie with specified name. If the cookie does not exists the method returns null.
      ''' </summary>
      ''' <param name="name"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Cookie(name As String) As Common.HttpCookie
         Get
            Return If(Not String.IsNullOrEmpty(name) AndAlso _cookies.ContainsKey(name), _cookies(name), Nothing)
         End Get
      End Property
      Private _cookies As New Dictionary(Of String, Common.HttpCookie)
      Public ReadOnly Property Cookies As HttpCookie()
         Get
            Return _cookies.Values.ToArray()
         End Get
      End Property
      
      ''' <summary>
      ''' Returns the pattern for a string expression (may or may not be doublequoted)
      ''' </summary>
      ''' <param name="groupName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shared Function GetStringPattern(groupName As String) As String
         Return "(""(?<" & groupName & ">(""""|[^""])*)""|(?<" & groupName & ">[^,;\s]*))"
      End Function

      Private _header As String
      Private _headers As System.Net.WebHeaderCollection

      ''' <summary>
      ''' Updates headers if a cookie is modified
      ''' </summary>
      ''' <remarks></remarks>
      Friend Sub Refresh()
         If Not (String.IsNullOrEmpty(_header) OrElse _headers Is Nothing) Then
            _headers.Remove(_header)
            _headers.Add(_header, String.Join(",", (From cookie As HttpCookie In _cookies.Values
                                                    Let value As String = cookie.ToString()
                                                    Select value)))
         End If
      End Sub

      ''' <summary>
      ''' Uri.UnescapeDataString()
      ''' </summary>
      Private Function TryUnescapeDataString(value As String) As String
         Try
            If String.IsNullOrEmpty(value) Then
               Return value
            Else
               Return Uri.UnescapeDataString(value)
            End If
         Catch ex As Exception
            Return value
         End Try
      End Function

      ''' <summary>
      ''' Returns the value of the specified cookie. If the cookie does not exists the function returns null.
      ''' </summary>
      Public ReadOnly Property Value(name As String) As String
         Get
            Dim cookie As HttpCookie = Me.Cookie(name)
            Return If(cookie Is Nothing, Nothing, cookie.Value)
         End Get
      End Property

   End Class
End Namespace