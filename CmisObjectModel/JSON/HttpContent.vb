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

Namespace CmisObjectModel.JSON
   ''' <summary>
   ''' A simple class to store content data separated from multipart content data
   ''' </summary>
   ''' <remarks></remarks>
   Public Class HttpContent

      Protected Const charsetPattern As String = "charset\=(?<" & charsetGroupName & ">[^;\r\n]*)"
      Protected Const charsetGroupName As String = "charset"

      Public Sub New(rawData As Byte())
         Me.RawData = rawData
         _value = rawData
      End Sub

#Region "Helper classes"
      ''' <summary>
      ''' Simple header collection that triggers the owners Reset()-method on content changes
      ''' </summary>
      ''' <remarks></remarks>
      Public Class HeadersCollection
         Implements IDictionary(Of String, String)

         Public Sub New(owner As HttpContent)
            _owner = owner
         End Sub

#Region "IDictionary"
         Public Sub Add(item As System.Collections.Generic.KeyValuePair(Of String, String)) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Add
            Me.Item(item.Key) = item.Value
         End Sub

         Public Sub Add(key As String, value As String) Implements System.Collections.Generic.IDictionary(Of String, String).Add
            Item(key) = value
         End Sub

         Public Sub Clear() Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Clear
            _headers.Clear()
         End Sub

         Public Function Contains(item As System.Collections.Generic.KeyValuePair(Of String, String)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Contains
            Return item.Key IsNot Nothing AndAlso _headers.ContainsKey(item.Key) AndAlso item.Value = _headers(item.Key)
         End Function

         Public Function ContainsKey(key As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, String).ContainsKey
            Return key IsNot Nothing AndAlso _headers.ContainsKey(key)
         End Function

         Public Sub CopyTo(array() As System.Collections.Generic.KeyValuePair(Of String, String), arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).CopyTo
            CType(_headers, IDictionary(Of String, String)).CopyTo(array, arrayIndex)
         End Sub

         Public ReadOnly Property Count As Integer Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Count
            Get
               Return _headers.Count
            End Get
         End Property

         Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of System.Collections.Generic.KeyValuePair(Of String, String)) Implements System.Collections.Generic.IEnumerable(Of System.Collections.Generic.KeyValuePair(Of String, String)).GetEnumerator
            Return _headers.GetEnumerator()
         End Function

         Private Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return GetEnumerator()
         End Function

         Public ReadOnly Property IsReadOnly As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).IsReadOnly
            Get
               Return CType(_headers, IDictionary(Of String, String)).IsReadOnly
            End Get
         End Property

         Default Public Property Item(key As String) As String Implements System.Collections.Generic.IDictionary(Of String, String).Item
            Get
               Return If(key IsNot Nothing AndAlso _headers.ContainsKey(key), _headers(key), Nothing)
            End Get
            Set(value As String)
               If _headers.ContainsKey(key) Then
                  _headers(key) = value
               Else
                  _headers.Add(key, value)
               End If
               _owner.Reset()
            End Set
         End Property

         Public ReadOnly Property Keys As System.Collections.Generic.ICollection(Of String) Implements System.Collections.Generic.IDictionary(Of String, String).Keys
            Get
               Return _headers.Keys
            End Get
         End Property

         Public Function Remove(item As System.Collections.Generic.KeyValuePair(Of String, String)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, String)).Remove
            Return Contains(item) AndAlso _headers.Remove(item.Key)
         End Function

         Public Function Remove(key As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, String).Remove
            Return _headers.Remove(key)
         End Function

         Public Function TryGetValue(key As String, ByRef value As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, String).TryGetValue
            Return _headers.TryGetValue(key, value)
         End Function

         Public ReadOnly Property Values As System.Collections.Generic.ICollection(Of String) Implements System.Collections.Generic.IDictionary(Of String, String).Values
            Get
               Return _headers.Values
            End Get
         End Property
#End Region

         Private _headers As New Dictionary(Of String, String)
         Private _owner As HttpContent
      End Class
#End Region

      Private Shared _contentDispositionRegEx As New System.Text.RegularExpressions.Regex("form-data;\s+name=""(?<Name>[^""]*)""",
                                                                                          Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                                                                          Text.RegularExpressions.RegexOptions.IgnoreCase)
      Public ReadOnly Property ContentDisposition As String
         Get
            SyncLock _contentDispositionRegEx
               Dim headerRawValue As String = If(Headers.ContainsKey(Common.RFC2231Helper.ContentDispositionHeaderName),
                                                 Headers(Common.RFC2231Helper.ContentDispositionHeaderName), Nothing)
               If Not String.IsNullOrEmpty(headerRawValue) Then
                  Dim match As System.Text.RegularExpressions.Match = _contentDispositionRegEx.Match(headerRawValue)
                  Return If(match Is Nothing OrElse Not match.Success, Nothing, match.Groups("Name").Value)
               Else
                  Return Nothing
               End If
            End SyncLock
         End Get
      End Property

      ''' <summary>
      ''' Gets or sets the contenttype of this instance
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Property ContentType As String
         Get
            Return If(Headers.ContainsKey(Common.RFC2231Helper.ContentTypeHeaderName), Headers(Common.RFC2231Helper.ContentTypeHeaderName), Nothing)
         End Get
         Set(value As String)
            If String.IsNullOrEmpty(value) Then
               Headers.Remove(Common.RFC2231Helper.ContentTypeHeaderName)
            ElseIf Headers.ContainsKey(Common.RFC2231Helper.ContentTypeHeaderName) Then
               Headers(Common.RFC2231Helper.ContentTypeHeaderName) = value
            Else
               Headers.Add(Common.RFC2231Helper.ContentTypeHeaderName, value)
            End If
         End Set
      End Property

      ''' <summary>
      ''' Encodes RawData with given encoding
      ''' </summary>
      ''' <param name="encoding"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function GetString(encoding As System.Text.Encoding) As String
         If RawData Is Nothing Then
            Return Nothing
         ElseIf RawData.Length = 0 Then
            Return String.Empty
         Else
            Return encoding.GetString(RawData)
         End If
      End Function

      Public ReadOnly Headers As New HeadersCollection(Me)

      Public ReadOnly Property IsBinary As Boolean
         Get
            Return _toStringResult Is _toStringResultDefault
         End Get
      End Property

      Public ReadOnly RawData As Byte()

      ''' <summary>
      ''' Sets Value and ToStringResult dependent on header information
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub Reset()
         Dim contentType As String = If(Headers.ContainsKey(Common.RFC2231Helper.ContentTypeHeaderName),
                                        Headers(Common.RFC2231Helper.ContentTypeHeaderName), Nothing)
         Dim contentTransferEncoding As String = If(Headers.ContainsKey(Common.RFC2231Helper.ContentTransferEncoding),
                                                    Headers(Common.RFC2231Helper.ContentTransferEncoding), Nothing)
         Dim regExCharset As New System.Text.RegularExpressions.Regex(charsetPattern, Text.RegularExpressions.RegexOptions.ExplicitCapture Or Text.RegularExpressions.RegexOptions.Singleline)
         Dim match As System.Text.RegularExpressions.Match = regExCharset.Match(If(contentType, String.Empty))
         Dim charset As String = If(match Is Nothing, Nothing, match.Groups(charsetGroupName).Value)

         Select Case If(contentTransferEncoding, String.Empty).ToLowerInvariant()
            Case String.Empty
               'text
               Try
                  Dim encoding As System.Text.Encoding = If(String.IsNullOrEmpty(charset),
                                                            System.Text.Encoding.GetEncoding(850),
                                                            System.Text.Encoding.GetEncoding(charset))
                  Dim toString As String = GetString(encoding)
                  _toStringResult = Function() toString
               Catch ex As Exception
                  _toStringResult = _toStringResultDefault
               End Try
               _value = RawData
            Case "7bit"
               'value 'as is'
               _value = RawData
               'ascii
               Try
                  Dim toString As String = GetString(System.Text.Encoding.ASCII)
                  _toStringResult = Function() toString
               Catch ex As Exception
                  _toStringResult = _toStringResultDefault
               End Try
            Case "8bit"
               'value 'as is'
               _value = RawData
               'charset or cp850
               Try
                  Dim encoding As System.Text.Encoding = If(String.IsNullOrEmpty(charset),
                                                            System.Text.Encoding.GetEncoding(850),
                                                            System.Text.Encoding.GetEncoding(charset))
                  Dim toString As String = GetString(encoding)
                  _toStringResult = Function() toString
               Catch ex As Exception
                  _toStringResult = _toStringResultDefault
               End Try
            Case "base64"
               Try
                  Dim encoding As System.Text.Encoding = If(String.IsNullOrEmpty(charset),
                                                            System.Text.Encoding.UTF8,
                                                            System.Text.Encoding.GetEncoding(charset))
                  Dim toString As String = GetString(encoding)

                  If RawData Is Nothing Then
                     _value = Nothing
                  Else
                     _value = System.Convert.FromBase64String(toString)
                  End If
                  _toStringResult = Function() toString
               Catch ex As Exception
                  _value = RawData
                  _toStringResult = _toStringResultDefault
               End Try
            Case "binary"
               'value 'as is'
               _value = RawData
               'no text representation
               _toStringResult = _toStringResultDefault
            Case "quoted-printable"
               Try
                  Dim encoding As System.Text.Encoding = If(String.IsNullOrEmpty(charset),
                                                            System.Text.Encoding.UTF8,
                                                            System.Text.Encoding.GetEncoding(charset))
                  Dim toString As String = GetString(encoding)

                  If RawData Is Nothing Then
                     _value = Nothing
                  Else
                     Dim regEx As New System.Text.RegularExpressions.Regex("(\r\n|=(?<Hex>[a-f0-9]{2}))", Text.RegularExpressions.RegexOptions.Singleline)
                     Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
                        Function(current)
                           If current.Value = vbCrLf Then
                              Return Nothing
                           Else
                              Return Chr(CType(System.Convert.ToUInt32(current.Value, 16), Integer))
                           End If
                        End Function
                     toString = regEx.Replace(toString, evaluator)
                     _value = encoding.GetBytes(toString)
                  End If
                  _toStringResult = Function() toString
               Catch ex As Exception
                  _value = RawData
                  _toStringResult = _toStringResultDefault
               End Try
         End Select
      End Sub

      Private _toStringResult As Func(Of String) = Function() MyBase.ToString()
      Private _toStringResultDefault As Func(Of String) = _toStringResult
      Public Overrides Function ToString() As String
         Return _toStringResult.Invoke()
      End Function

      Private _value As Byte()
      Public ReadOnly Property Value As Byte()
         Get
            Return _value
         End Get
      End Property

      ''' <summary>
      ''' Writes the headers
      ''' </summary>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteHeaders(stream As IO.Stream)
         For Each de As KeyValuePair(Of String, String) In Headers
            Dim buffer As Byte() = System.Text.Encoding.UTF8.GetBytes(de.Key & ": " & de.Value & vbCrLf)
            stream.Write(buffer, 0, buffer.Length)
         Next
         WriteLine(stream)
      End Sub

      ''' <summary>
      ''' Writes a CrLf into the stream
      ''' </summary>
      ''' <param name="stream"></param>
      ''' <remarks></remarks>
      Protected Sub WriteLine(stream As IO.Stream)
         Dim buffer As Byte() = System.Text.Encoding.UTF8.GetBytes(vbCrLf)
         stream.Write(buffer, 0, buffer.Length)
      End Sub

      ''' <summary>
      ''' Writes current content to stream
      ''' </summary>
      ''' <param name="stream"></param>
      ''' <remarks></remarks>
      Public Sub WriteTo(stream As IO.Stream)
         WriteHeaders(stream)
         WriteValue(stream)
      End Sub

      ''' <summary>
      ''' Writes current content to streamwriter
      ''' </summary>
      Protected Overridable Sub WriteValue(stream As IO.Stream)
         If Value IsNot Nothing Then
            stream.Write(Value, 0, Value.Length)
            WriteLine(stream)
         End If
      End Sub


   End Class
End Namespace