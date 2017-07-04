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
Imports ccg = CmisObjectModel.Common.Generic
Imports st = System.Text
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   ''' <summary>
   ''' Represents a constant expression (string or number)
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ConstantExpression
      Inherits Expression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index)
      End Sub
#End Region

      ''' <summary>
      ''' Returns the boolean value if the constant was detected as boolean, otherwise null
      ''' </summary>
      Public ReadOnly Property BooleanConstant As Boolean?
         Get
            Dim group = Match.Groups("BooleanConstant")

            If group Is Nothing OrElse Not group.Success Then
               Return Nothing
            Else
               Return Boolean.Parse(group.Value)
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns the date value if the constant was detected as date, otherwise null
      ''' </summary>
      Public ReadOnly Property DateConstant As DateTimeOffset?
         Get
            Dim group = Match.Groups("DateConstant")

            If group Is Nothing OrElse Not group.Success Then
               Return Nothing
            Else
               Dim groupHour = Match.Groups("Hour")

               If group Is Nothing OrElse Not group.Success Then
                  'only a date, no time information
                  Return New DateTime(CInt(Match.Groups("Year").Value), CInt(Match.Groups("Month").Value), CInt(Match.Groups("Day").Value))
               Else
                  Dim groupMillisecond = Match.Groups("Millisecond")
                  Dim groupOffset = Match.Groups("Offset")
                  Dim millisecond As Integer = If(groupMillisecond Is Nothing OrElse Not groupMillisecond.Success, 0,
                                                  CInt(groupMillisecond.Value.PadRight(3, "0"c)))
                  Dim baseDate As New DateTime(CInt(Match.Groups("Year").Value), CInt(Match.Groups("Month").Value), CInt(Match.Groups("Day").Value),
                                               CInt(Match.Groups("Hour").Value), CInt(Match.Groups("Minute").Value), CInt(Match.Groups("Second").Value), millisecond,
                                               If(groupOffset Is Nothing OrElse Not groupOffset.Success, DateTimeKind.Local, DateTimeKind.Utc))
                  Dim groupOffsetUtc = Match.Groups("OffsetUtc")

                  If groupOffset Is Nothing OrElse Not groupOffset.Success OrElse
                     groupOffsetUtc IsNot Nothing AndAlso groupOffsetUtc.Success Then
                     Return baseDate
                  Else
                     Dim sign As Integer = If(Match.Groups("OffsetSign").Value = "-", -1, 1)
                     Dim offset As New TimeSpan(sign + CInt(Match.Groups("OffsetHour").Value), sign * CInt(Match.Groups("OffsetMinute").Value), 0)

                     Return New DateTimeOffset(baseDate, offset)
                  End If
               End If
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns the double value if the constant was detected as number, otherwise null
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property NumberConstant As Double?
         Get
            Dim group = Match.Groups("NumberConstant")

            If group Is Nothing OrElse Not group.Success Then
               Return Nothing
            Else
               Return Double.Parse(group.Value)
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns the string value if the constant was detected as string, otherwise null
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property StringConstant As ccg.Nullable(Of String)
         Get
            Dim group = Match.Groups("StringConstant")

            If group Is Nothing OrElse Not group.Success Then
               Return Nothing
            Else
               Return group.Value
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns null if the constant was not detected as string, otherwise
      ''' following translations were made:
      ''' '' => '
      ''' In case of a contains() expression following additional translations were made:
      ''' \\ => \, \' => ', \r => carriage return, \n => linefeed, \t => tabulator,
      ''' \uXXXX => unicode character ChrW(XXXX)
      ''' </summary>
      Public ReadOnly Property UnescapedStringConstant As ccg.Nullable(Of String)
         Get
            Dim stringConstant As ccg.Nullable(Of String) = Me.StringConstant

            If String.IsNullOrEmpty(stringConstant.Value) Then
               Return stringConstant
            Else
               Dim group = Match.Groups("IsContainsOperation")

               If group Is Nothing OrElse Not group.Success Then
                  Return stringConstant.Value.Replace("''", "'")
               Else
                  Dim regEx As New System.Text.RegularExpressions.Regex("(\\u(?<Unicode>\d{1,4})|\\(?<EscapedChar>.)|'(?<EscapedChar>'))", Text.RegularExpressions.RegexOptions.Singleline)
                  Dim evaluator As System.Text.RegularExpressions.MatchEvaluator = Function(match)
                                                                                      Dim unicodeGroup As System.Text.RegularExpressions.Group = match.Groups("Unicode")

                                                                                      If unicodeGroup Is Nothing OrElse Not unicodeGroup.Success Then
                                                                                         Dim escapedChar As String = match.Groups("EscapedChar").Value
                                                                                         Select Case escapedChar
                                                                                            Case "r"
                                                                                               Return vbCr
                                                                                            Case "n"
                                                                                               Return vbLf
                                                                                            Case "t"
                                                                                               Return vbTab
                                                                                            Case Else
                                                                                               Return escapedChar
                                                                                         End Select
                                                                                      Else
                                                                                         Return ChrW(CInt(unicodeGroup.Value))
                                                                                      End If
                                                                                   End Function
                  Return regEx.Replace(stringConstant.Value, evaluator)
               End If
            End If
         End Get
      End Property

   End Class
End Namespace