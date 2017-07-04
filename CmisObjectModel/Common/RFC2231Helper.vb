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
   ''' Decodes and encodes key-value assignments
   ''' </summary>
   ''' <remarks></remarks>
   Public Module RFC2231Helper

      Private Const _mimeSpecials As String = "()<>@,;:\""/[]?= " + vbTab
      Private Const _rfc2231Specials As String = "*'%" + _mimeSpecials
      Private _hexDigits As Char() = New Char() {"0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c,
                                                 "A"c, "B"c, "C"c, "D"c, "E"c, "F"c}

#Region "Content-Disposition"
      Public Const ContentDispositionHeaderName As String = "Content-Disposition"
      Public Const ContentTransferEncoding As String = "Content-Transfer-Encoding"
      Public Const ContentTypeHeaderName As String = "Content-Type"
      Private Const _attachment As String = "attachment"
      Private Const _fileName As String = "filename"

      ''' <summary>
      ''' Tries to evaluate expression as a content-disposition assignment
      ''' </summary>
      ''' <param name="expression"></param>
      ''' <param name="disposition"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function DecodeContentDisposition(expression As String, ByRef disposition As String) As String
         Dim regEx As New System.Text.RegularExpressions.Regex("(?<Disposition>[^;]*);\s" & _fileName & "\s*(?<Encoded>\*)?\s*=\s*((?<=\*\s*=\s*)(?<Encoding>[^']*)'(?<Language>[^']*)')?(?<FileName>(%(?<Char>[0-9A-F]{2})|[\s\S])*)",
                                                               Text.RegularExpressions.RegexOptions.Singleline Or
                                                               Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                                               Text.RegularExpressions.RegexOptions.IgnoreCase)
         If Not String.IsNullOrEmpty(expression) Then
            Dim match As System.Text.RegularExpressions.Match = regEx.Match(expression)

            If match IsNot Nothing AndAlso match.Success Then
               Dim encoding As System.Text.Encoding = GetEncoding(match.Groups("Encoding"))
               Dim group As System.Text.RegularExpressions.Group = match.Groups("Char")

               disposition = match.Groups("Disposition").Value
               If group Is Nothing OrElse Not group.Success Then
                  Return match.Groups("FileName").Value
               ElseIf encoding Is Nothing Then
                  Dim regExDecode As New System.Text.RegularExpressions.Regex("%(?<Char>[0-9A-F]{2})",
                                                                              Text.RegularExpressions.RegexOptions.Singleline Or
                                                                              Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                                                              Text.RegularExpressions.RegexOptions.IgnoreCase)
                  Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
                     Function(currentMatch)
                        Return ChrW(System.Convert.ToInt32(currentMatch.Groups("Char").Value, 16))
                     End Function
                  Return regExDecode.Replace(match.Groups("FileName").Value, evaluator)
               Else
                  Dim regExDecode As New System.Text.RegularExpressions.Regex("(%(?<Char>[0-9A-F]{2}))+",
                                                                              Text.RegularExpressions.RegexOptions.Singleline Or
                                                                              Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                                                              Text.RegularExpressions.RegexOptions.IgnoreCase)
                  Dim evaluator As System.Text.RegularExpressions.MatchEvaluator =
                     Function(currentMatch)
                        With currentMatch.Groups("Char")
                           Dim bytes As Byte() = (From item As Object In .Captures
                                                  Let capture As System.Text.RegularExpressions.Capture = DirectCast(item, System.Text.RegularExpressions.Capture)
                                                  Select System.Convert.ToByte(capture.Value, 16)).ToArray()
                           Return encoding.GetString(bytes)
                        End With
                     End Function
                  Return regExDecode.Replace(match.Groups("FileName").Value, evaluator)
               End If
            End If
         End If

         disposition = Nothing
         Return Nothing
      End Function

      ''' <summary>
      ''' Encodes given fileName and dispositionType as content-disposition assignment
      ''' </summary>
      ''' <param name="filename"></param>
      ''' <param name="dispositionType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function EncodeContentDisposition(filename As String, Optional dispositionType As String = _attachment) As String
         If dispositionType Is Nothing Then dispositionType = _attachment
         Return dispositionType & "; " & EncodeKeyValuePair(_fileName, filename)
      End Function
#End Region

      ''' <summary>
      ''' Encodes a key-value-pair
      ''' </summary>
      ''' <param name="key"></param>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function EncodeKeyValuePair(key As String, value As String) As String
         Return key & If(EncodeValue(value), "*", "") & "=" & value
      End Function

      ''' <summary>
      ''' Encodes a value
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function EncodeValue(ByRef value As String) As Boolean
         Dim sb As New System.Text.StringBuilder
         Dim encoded As Boolean = False

         sb.Append("UTF-8")
         sb.Append("''") ' no language
         Try
            Dim bytes As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(value)

            For index As Integer = 0 To bytes.Length - 1
               Dim ch As Integer = bytes(index) And &HFF
               If ch <= 32 OrElse ch >= 127 OrElse _rfc2231Specials.IndexOf(ChrW(ch)) <> -1 Then
                  sb.Append("%"c)
                  sb.Append(_hexDigits(ch >> 4))
                  sb.Append(_hexDigits(ch And &HF))
                  encoded = True
               Else
                  sb.Append(ChrW(ch))
               End If
            Next

            If encoded Then
               value = sb.ToString()
               Return True
            Else
               Return False
            End If
         Catch
            value = sb.ToString()
            Return True
         End Try
      End Function

      ''' <summary>
      ''' Tries to convert grEncoding.Value to the corresponding Encoding
      ''' </summary>
      Private Function GetEncoding(grEncoding As System.Text.RegularExpressions.Group) As System.Text.Encoding
         Dim name As String = If(grEncoding Is Nothing OrElse Not grEncoding.Success, Nothing, grEncoding.Value)
         Try
            Return If(String.IsNullOrEmpty(name), Nothing, System.Text.Encoding.GetEncoding(name))
         Catch
            Return Nothing
         End Try
      End Function
   End Module
End Namespace