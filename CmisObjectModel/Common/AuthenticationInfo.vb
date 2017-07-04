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
Imports ssw = System.ServiceModel.Web

Namespace CmisObjectModel.Common
   ''' <summary>
   ''' Simple class to encapsulate information about user and password
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AuthenticationInfo

#Region "Constructors"
      Protected Sub New(user As String, password As System.Security.SecureString)
         _user = user
         _password = password
      End Sub

      ''' <summary>
      ''' Creates a new instance
      ''' </summary>
      ''' <param name="user"></param>
      ''' <param name="password"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(user As String, password As System.Security.SecureString) As AuthenticationInfo
         Return New AuthenticationInfo(user, password)
      End Function
#End Region

      ''' <summary>
      ''' Copies the password into given utf8Bytes-buffer
      ''' </summary>
      ''' <param name="utf8Bytes"></param>
      ''' <remarks></remarks>
      Public Sub CopyPasswordTo(utf8Bytes As List(Of Byte))
         CopyPasswordTo(_password, utf8Bytes)
      End Sub
      ''' <summary>
      ''' Copies the password into given utf8Bytes-buffer
      ''' </summary>
      ''' <param name="password"></param>
      ''' <param name="utf8Bytes"></param>
      ''' <remarks></remarks>
      Public Shared Sub CopyPasswordTo(password As System.Security.SecureString, utf8Bytes As List(Of Byte))
         Dim chars As New List(Of Char)

         CopyPasswordTo(password, chars)
         If chars.Count > 0 Then utf8Bytes.AddRange(System.Text.Encoding.UTF8.GetBytes(chars.ToArray()))
      End Sub

      ''' <summary>
      ''' Copies the password into given chars-buffer
      ''' </summary>
      ''' <param name="chars"></param>
      ''' <remarks></remarks>
      Public Sub CopyPasswordTo(chars As List(Of Char))
         CopyPasswordTo(_password, chars)
      End Sub
      ''' <summary>
      ''' Copies the password into given chars-buffer
      ''' </summary>
      ''' <param name="password"></param>
      ''' <param name="chars"></param>
      ''' <remarks></remarks>
      Public Shared Sub CopyPasswordTo(password As System.Security.SecureString, chars As List(Of Char))
         If password IsNot Nothing AndAlso password.Length > 0 Then
            Dim valuePtr As IntPtr = IntPtr.Zero

            Try
               valuePtr = sri.Marshal.SecureStringToGlobalAllocUnicode(password)
               For index As Integer = 0 To password.Length - 1
                  chars.Add(ChrW(sri.Marshal.ReadInt16(valuePtr, index << 1)))
               Next
            Finally
               If Not valuePtr.Equals(IntPtr.Zero) Then sri.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr)
            End Try
         End If
      End Sub

      ''' <summary>
      ''' Returns authenticationInfo from the current incoming web-request
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function FromCurrentWebRequest() As AuthenticationInfo
         Return FromCurrentWebRequest(AddressOf CreateInstance)
      End Function
      ''' <summary>
      ''' Returns authenticationInfo from the current incoming web-request
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function FromCurrentWebRequest(Of TResult As AuthenticationInfo)(factory As Func(Of String, System.Security.SecureString, TResult)) As TResult
         Dim authentication As String = ssw.WebOperationContext.Current.IncomingRequest.Headers(sn.HttpRequestHeader.Authorization)
         Dim regEx As New System.Text.RegularExpressions.Regex("Basic (?<Authentication>[\s\S]+)",
                                                               Text.RegularExpressions.RegexOptions.IgnoreCase Or
                                                               Text.RegularExpressions.RegexOptions.Singleline Or
                                                               Text.RegularExpressions.RegexOptions.ExplicitCapture)
         Dim match As System.Text.RegularExpressions.Match = If(String.IsNullOrEmpty(authentication), Nothing, regEx.Match(authentication))

         If match Is Nothing OrElse Not match.Success Then
            Return factory.Invoke(Nothing, Nothing)
         Else
            Dim chars As Char() = System.Text.Encoding.UTF8.GetChars(System.Convert.FromBase64String(match.Groups("Authentication").Value))
            Dim sbUser As New System.Text.StringBuilder
            Dim password As New System.Security.SecureString
            Dim infoSelector As Boolean = True

            For Each ch As Char In chars
               If infoSelector Then
                  If ch = ":"c Then
                     'password starts from this point
                     infoSelector = False
                  Else
                     sbUser.Append(ch)
                  End If
               Else
                  password.AppendChar(ch)
               End If
            Next
            Return factory.Invoke(sbUser.ToString(), password)
         End If
      End Function

      ''' <summary>
      ''' Returns password as char-array
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetPasswordChars() As Char()
         Dim chars As New List(Of Char)

         CopyPasswordTo(chars)
         Return chars.ToArray
      End Function

      Protected ReadOnly _password As System.Security.SecureString
      Public ReadOnly Property Password As System.Security.SecureString
         Get
            Return _password
         End Get
      End Property

      ''' <summary>
      ''' Returns the password as char-array
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function PasswordToCharArray() As Char()
         Return PasswordToCharArray(_password)
      End Function
      ''' <summary>
      ''' Returns the given password as char-array
      ''' </summary>
      ''' <param name="password"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function PasswordToCharArray(password As System.Security.SecureString) As Char()
         Dim chars As New List(Of Char)

         CopyPasswordTo(password, chars)
         Return chars.ToArray
      End Function

      ''' <summary>
      ''' Returns password as utf8Byte-array
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function PasswordToUtf8ByteArray() As Byte()
         Return PasswordToUtf8ByteArray(_password)
      End Function
      ''' <summary>
      ''' Returns the given password as utf8Byte-array
      ''' </summary>
      ''' <param name="password"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function PasswordToUtf8ByteArray(password As System.Security.SecureString) As Byte()
         Dim utf8Buffer As New List(Of Byte)

         CopyPasswordTo(password, utf8Buffer)
         Return utf8Buffer.ToArray
      End Function

      Protected ReadOnly _user As String
      Public ReadOnly Property User As String
         Get
            Return _user
         End Get
      End Property
   End Class
End Namespace