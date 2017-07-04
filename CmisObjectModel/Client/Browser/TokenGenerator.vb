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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client.Browser
   ''' <summary>
   ''' Simple TokenGenerator for CmisObjectModel driven CMIS servers
   ''' </summary>
   ''' <remarks></remarks>
   Public Class TokenGenerator

#Region "Constructors"
      Protected Sub New()
      End Sub

      Public Sub New(fnGetCookies As Func(Of System.Net.CookieContainer), uri As Uri, sessionIdCookieName As String)
         If Not (String.IsNullOrEmpty(sessionIdCookieName) OrElse uri Is Nothing) Then
            _fnGetCookies = fnGetCookies
            _sessionIdCookieName = sessionIdCookieName
            _uri = uri
         Else
            _fnGetCookies = Nothing
         End If
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Implementation of an unchangeable token
      ''' </summary>
      ''' <remarks></remarks>
      Private Class StaticToken
         Inherits TokenGenerator

         Public Sub New(token As String)
            _token = token
         End Sub

         Public Overrides Function NextToken() As String
            Return _token
         End Function

         Private _token As String
      End Class
#End Region

      ''' <summary>
      ''' Sets the current token generator for the current thread
      ''' </summary>
      Public Shared Sub BeginToken(token As TokenGenerator)
         Dim thread As System.Threading.Thread = System.Threading.Thread.CurrentThread

         SyncLock _tokenStacks
            Dim stack As Stack(Of TokenGenerator)

            If _tokenStacks.ContainsKey(thread) Then
               stack = _tokenStacks(thread)
            Else
               stack = New Stack(Of TokenGenerator)
               _tokenStacks.Add(thread, stack)
            End If
            stack.Push(token)
         End SyncLock
      End Sub

      ''' <summary>
      ''' Returns the current token generator for the current thread
      ''' </summary>
      Public Shared ReadOnly Property Current As TokenGenerator
         Get
            Dim thread As System.Threading.Thread = System.Threading.Thread.CurrentThread

            SyncLock _tokenStacks
               Return If(_tokenStacks.ContainsKey(thread), _tokenStacks(thread).Peek(), Nothing)
            End SyncLock
         End Get
      End Property

      ''' <summary>
      ''' Removes the current token generator for the current thread
      ''' </summary>
      Public Shared Function EndToken() As TokenGenerator
         Dim thread As System.Threading.Thread = System.Threading.Thread.CurrentThread
         Dim retVal As TokenGenerator

         SyncLock _tokenStacks
            If _tokenStacks.ContainsKey(thread) Then
               Dim stack As Stack(Of TokenGenerator) = _tokenStacks(thread)
               Dim count As Integer = stack.Count

               If count > 0 Then
                  retVal = stack.Pop()
                  count -= 1
               Else
                  retVal = Nothing
               End If
               If count = 0 Then _tokenStacks.Remove(thread)
            Else
               retVal = Nothing
            End If
         End SyncLock

         Return retVal
      End Function

      Private _fnGetCookies As Func(Of System.Net.CookieContainer)

      ''' <summary>
      ''' Returns the next token (sessionId + "\r\n" + Guid.NewGuid.ToString())
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function NextToken() As String
         Dim cookies As System.Net.CookieContainer = If(_fnGetCookies Is Nothing, Nothing, _fnGetCookies.Invoke())
         Dim prefix As String = Nothing

         If cookies IsNot Nothing Then
            Dim cookieCollection As System.Net.CookieCollection = cookies.GetCookies(_uri)
            Dim cookie As System.Net.Cookie = If(cookieCollection Is Nothing, Nothing, cookieCollection.Item(_sessionIdCookieName))

            If cookie IsNot Nothing Then prefix = cookie.Value & vbCrLf
         End If

         Return prefix & System.Guid.NewGuid.ToString()
      End Function

      Private _sessionIdCookieName As String
      Private Shared _tokenStacks As New Dictionary(Of System.Threading.Thread, Stack(Of TokenGenerator))
      Private _uri As Uri

      Public Shared Widening Operator CType(value As TokenGenerator) As String
         Return If(value Is Nothing, Nothing, value.NextToken())
      End Operator

      Public Shared Widening Operator CType(value As String) As TokenGenerator
         Return New StaticToken(value)
      End Operator

   End Class
End Namespace