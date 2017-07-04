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

Namespace CmisObjectModel.Common
   ''' <summary>
   ''' Simple cookie class
   ''' </summary>
   ''' <remarks></remarks>
   Public Class HttpCookie

#Region "Constructors"
      Public Sub New(name As String)
         _name = name
      End Sub

      Public Sub New(name As String, value As String)
         Me.New(name)
         Me.Value = value
      End Sub
#End Region

      Public Sub AddExtension(key As String, value As String)
         If Not String.IsNullOrEmpty(key) Then
            _extensions.Remove(key)
            _extensions.Add(key, value)
         End If
      End Sub

      Private _domain As String
      Public Property Domain As String
         Get
            Return _domain
         End Get
         Set(value As String)
            _domain = value
            RefreshOwner()
         End Set
      End Property

      Private _expires As DateTime?
      Public Property Expires As DateTime?
         Get
            Return _expires
         End Get
         Set(value As DateTime?)
            _expires = value
            RefreshOwner()
         End Set
      End Property

      Public ReadOnly Property Extension(key As String) As String
         Get
            Return If(Not String.IsNullOrEmpty(key) AndAlso _extensions.ContainsKey(key), _extensions(key), Nothing)
         End Get
      End Property
      Private _extensions As New Dictionary(Of String, String)
      Public ReadOnly Property Extensions As KeyValuePair(Of String, String)()
         Get
            Return _extensions.ToArray()
         End Get
      End Property

      Private _httpOnly As Boolean
      Public Property HttpOnly As Boolean
         Get
            Return _httpOnly
         End Get
         Set(value As Boolean)
            _httpOnly = value
            RefreshOwner()
         End Set
      End Property

      Private _maxAge As Integer?
      Public Property MaxAge As Integer?
         Get
            Return _maxAge
         End Get
         Set(value As Integer?)
            _maxAge = value
            RefreshOwner()
         End Set
      End Property

      Private _name As String
      Public ReadOnly Property Name As String
         Get
            Return _name
         End Get
      End Property

      Friend Property Owner As CmisObjectModel.Collections.HttpCookieContainer

      Private _path As String
      Public Property Path As String
         Get
            Return _path
         End Get
         Set(value As String)
            _path = value
            RefreshOwner()
         End Set
      End Property

      Private Sub RefreshOwner()
         If Owner IsNot Nothing Then Owner.Refresh()
      End Sub

      Private Shared _regEx As New System.Text.RegularExpressions.Regex("((?<DoubleQuoted>\A""(""""|[^""])*""\z)|["";,])", Text.RegularExpressions.RegexOptions.Singleline)

      Private _secure As Boolean
      Public Property Secure As Boolean
         Get
            Return _secure
         End Get
         Set(value As Boolean)
            _secure = value
            RefreshOwner()
         End Set
      End Property

      Public Overrides Function ToString() As String
         If String.IsNullOrEmpty(_name) Then
            Return Nothing
         Else
            Dim sb As New System.Text.StringBuilder(_name)

            sb.Append("=")
            If Not String.IsNullOrEmpty(Value) Then
               sb.Append(Uri.EscapeDataString(Value))
            End If
            If _expires.HasValue Then
               sb.Append("; Expires=")
               sb.Append(_expires.Value.ToString(System.Globalization.DateTimeFormatInfo.InvariantInfo.RFC1123Pattern, System.Globalization.DateTimeFormatInfo.InvariantInfo))
            End If
            If _maxAge.HasValue Then
               sb.Append("; Max-Age=")
               sb.Append(_maxAge.Value)
            End If
            If Not String.IsNullOrEmpty(_domain) Then
               sb.Append("; Domain=")
               sb.Append(_domain)
            End If
            If Not String.IsNullOrEmpty(_path) Then
               sb.Append("; Path=")
               sb.Append(_path)
            End If
            If _secure Then sb.Append("; Secure")
            If _httpOnly Then sb.Append("; HttpOnly")
            For Each de As KeyValuePair(Of String, String) In _extensions
               sb.Append("; ")
               sb.Append(de.Key)
               If de.Value IsNot Nothing Then
                  sb.Append("=")
                  'extensions always as doublequoted strings
                  sb.Append("""")
                  sb.Append(Uri.EscapeDataString(de.Value))
                  sb.Append("""")
               End If
            Next

            Return sb.ToString()
         End If
      End Function

      Private _value As String
      Public Property Value As String
         Get
            Return _value
         End Get
         Set(value As String)
            _value = value
            RefreshOwner()
         End Set
      End Property

   End Class
End Namespace