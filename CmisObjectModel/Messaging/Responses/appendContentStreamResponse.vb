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
Imports ca = CmisObjectModel.AtomPub
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Messaging.Responses
   Partial Public Class appendContentStreamResponse

      Public Sub New(objectId As String, changeToken As String)
         _objectId = objectId
         _changeToken = changeToken
      End Sub

      ''' <summary>
      ''' Creates a new instance from the reader
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks>If the reader points to an AtomEntry-instance, the ObjectId and ChangeToken properties are used
      ''' to create a appendContentStreamResponse-instance</remarks>
      Public Shared Function CreateInstance(reader As System.Xml.XmlReader) As appendContentStreamResponse
         reader.MoveToContent()
         If String.Compare(reader.LocalName, "entry", True) = 0 Then
            With ca.AtomEntry.CreateInstance(reader)
               Return New appendContentStreamResponse(.ObjectId, .ChangeToken)
            End With
         Else
            Dim retVal As New appendContentStreamResponse
            retVal.ReadXml(reader)
            Return retVal
         End If
      End Function

   End Class
End Namespace