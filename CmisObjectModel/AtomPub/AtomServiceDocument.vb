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
Imports sss = System.ServiceModel.Syndication
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.AtomPub
   ''' <summary>
   ''' Represents a cmis service document (response on a repositoryInfo-request)
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AtomServiceDocument
      Inherits sss.ServiceDocument

      Public Sub New()
         MyBase.New()

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Public Sub New(ParamArray workspaces As AtomWorkspace())
         Me.New()

         If workspaces IsNot Nothing Then
            For Each workspace As AtomWorkspace In workspaces
               'omit duplicate namespace definitions
               For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
                  workspace.AttributeExtensions.Remove(de.Key)
               Next
               Me.Workspaces.Add(workspace)
            Next
         End If
      End Sub

      ''' <summary>
      ''' Creates a new instance (similar to ReadXml() in IXmlSerializable-classes)
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As sx.XmlReader) As AtomServiceDocument
         Dim isEmptyElement As Boolean
         Dim retVal As New AtomServiceDocument

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         reader.ReadStartElement()
         If Not isEmptyElement Then
            reader.MoveToContent()
            While reader.IsStartElement
               Select Case reader.NamespaceURI
                  Case Constants.Namespaces.app
                     If String.Compare(reader.LocalName, "workspace", True) = 0 Then
                        Dim workspace As AtomWorkspace = AtomWorkspace.CreateInstance(reader)
                        If workspace IsNot Nothing Then retVal.Workspaces.Add(workspace)
                     Else
                        'ignore node
                        reader.ReadOuterXml()
                     End If
                  Case Else
                     'ignore node
                     reader.ReadOuterXml()
               End Select
               reader.MoveToContent()
            End While

            reader.ReadEndElement()
         End If

         Return retVal
      End Function

   End Class
End Namespace