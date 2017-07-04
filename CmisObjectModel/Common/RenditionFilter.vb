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
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web

Namespace CmisObjectModel.Common
   ''' <summary>
   ''' Implementation of the redition filter grammar
   ''' </summary>
   ''' <remarks>
   ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
   ''' </remarks>
   Public Class RenditionFilter
      Private Sub New(filter As String)
         'remove whitespaces
         If filter <> "" Then
            Dim regEx As New System.Text.RegularExpressions.Regex("\s")
            filter = regEx.Replace(filter, "")
         End If
         'evaluate filter expression
         If filter <> "" AndAlso filter <> "cmis:none" Then
            'comma separated terms
            For Each match As System.Text.RegularExpressions.Match In _regEx.Matches(filter)
               Dim grInvalid As System.Text.RegularExpressions.Group = match.Groups("Invalid")

               'type and subtype MUST NOT contain a whitespace
               If grInvalid Is Nothing OrElse Not grInvalid.Success Then
                  Dim type As String = match.Groups("Type").Value
                  Dim grSubType As System.Text.RegularExpressions.Group = match.Groups("SubType")
                  Dim subType As String = If(grSubType Is Nothing OrElse Not grSubType.Success, "", grSubType.Value)
                  Dim verify As Dictionary(Of String, Object)

                  If _filters.ContainsKey(type) Then
                     verify = _filters(type)
                  Else
                     verify = New Dictionary(Of String, Object)
                     _filters.Add(type, verify)
                  End If
                  If Not verify.ContainsKey(subType) Then verify.Add(subType, Nothing)
               End If
            Next
         End If
      End Sub

      Public Shared Widening Operator CType(value As String) As RenditionFilter
         Return New RenditionFilter(value)
      End Operator

      ''' <summary>
      ''' Returns True if mimeType is defined in this instance
      ''' </summary>
      ''' <param name="mimeType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ContainsMimeType(mimeType As String) As Boolean
         Dim match As System.Text.RegularExpressions.Match = _regEx.Match(If(mimeType Is Nothing, "", mimeType))

         If match IsNot Nothing AndAlso match.Success Then
            Dim grInvalid As System.Text.RegularExpressions.Group = match.Groups("Invalid")

            If grInvalid Is Nothing OrElse Not grInvalid.Success Then
               If _filters.ContainsKey("*") Then Return True
               If _filters.ContainsKey(match.Groups("Type").Value) Then
                  Dim verify As Dictionary(Of String, Object) = _filters(match.Groups("Type").Value)
                  Dim grSubType As System.Text.RegularExpressions.Group = match.Groups("SubType")
                  Dim subType As String = If(grSubType Is Nothing OrElse Not grSubType.Success, "", grSubType.Value)
                  Return verify.ContainsKey(subType) OrElse (subType <> "" AndAlso verify.ContainsKey("*"))
               End If
            End If
         End If

         Return False
      End Function

      Private _filters As New Dictionary(Of String, Dictionary(Of String, Object))
      Private _regEx As New System.Text.RegularExpressions.Regex("(?<Term>(?<Type>([^\s,\/]+|(?<Invalid>\s))+)(\/(?<SubType>([^\s,]+|(?<Invalid>[\s\/]))+))?)")

   End Class
End Namespace