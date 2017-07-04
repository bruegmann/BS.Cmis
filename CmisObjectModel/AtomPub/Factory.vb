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
Imports sxl = System.Xml.Linq
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.AtomPub
   ''' <summary>
   ''' Methods to convert XmlReader-nodes to specified types
   ''' </summary>
   ''' <remarks></remarks>
   Public Module Factory

#Region "Helper classes"
      Public MustInherit Class GenericDelegates(Of TParam, TResult)
         Public Delegate Function CreateLinkDelegate(uri As TParam, relationshipType As String, mediaType As String, id As String, renditionKind As String) As TResult
      End Class

      ''' <summary>
      ''' Implements the creation of XElement-type
      ''' </summary>
      ''' <remarks></remarks>
      Public Class XElementBuilder

         Public Sub New(ns As sxl.XNamespace, elementName As String)
            _ns = ns
            _elementName = elementName
         End Sub

         ''' <summary>
         ''' Creates XElement-instance specified by href, relationshipType, mediaType, id (cmisra:id) and renditionKind (cmisra:renditionKind)
         ''' </summary>
         ''' <param name="href"></param>
         ''' <param name="relationshipType"></param>
         ''' <param name="mediaType"></param>
         ''' <param name="id"></param>
         ''' <param name="renditionKind"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Function CreateXElement(href As String, relationshipType As String, mediaType As String,
                                        id As String, renditionKind As String) As sxl.XElement
            Dim contents As New List(Of Object) From {
               New sxl.XAttribute("type", mediaType),
               New sxl.XAttribute("rel", relationshipType),
               New sxl.XAttribute("href", href)}

            If id <> "" Then contents.Add(New sxl.XAttribute("{" & Constants.Namespaces.cmisra & "}id", id))
            If renditionKind <> "" Then contents.Add(New sxl.XAttribute("{" & Constants.Namespaces.cmisra & "}renditionKind", id))
            contents.Add(String.Empty)

            Return New sxl.XElement(_ns + _elementName,
                                    contents.ToArray())
         End Function

         Public Function CreateXElement(uri As Uri, relationshipType As String, mediaType As String,
                                        id As String, renditionKind As String) As sxl.XElement
            Return CreateXElement(uri.AbsoluteUri, relationshipType, mediaType, id, renditionKind)
         End Function

         Private _ns As sxl.XNamespace
         Private _elementName As String

      End Class
#End Region

      ''' <summary>
      ''' Evaluates the current node as a SyndicationPerson-instance
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateAuthor(reader As sx.XmlReader) As sss.SyndicationPerson
         reader.MoveToContent()
         If reader.IsEmptyElement Then
            reader.ReadOuterXml()
            Return New sss.SyndicationPerson()
         Else
            Dim name As String = Nothing
            Dim email As String = Nothing
            Dim uri As String = Nothing

            reader.ReadStartElement()
            reader.MoveToContent()
            While reader.IsStartElement
               Select Case reader.LocalName
                  Case "email"
                     email = reader.ReadElementString()
                  Case "name"
                     name = reader.ReadElementString()
                  Case "uri"
                     uri = reader.ReadElementString()
                  Case Else
                     'ignore node
                     reader.ReadOuterXml()
               End Select
               reader.MoveToContent()
            End While
            reader.ReadEndElement()
            Return New sss.SyndicationPerson(email, name, uri)
         End If
      End Function

      Private _toDateTimeRegEx As New System.Text.RegularExpressions.Regex("-?\d{4,}(-\d{2}){2}T\d{2}(\:\d{2}){2}(\.\d+)?([\+\-]\d{2}\:\d{2}|Z)?",
                                                                           Text.RegularExpressions.RegexOptions.Singleline Or
                                                                           Text.RegularExpressions.RegexOptions.IgnoreCase)
      ''' <summary>
      ''' Evaluates the current node as a DateTimeOffset-value
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateDateTimeOffset(reader As sx.XmlReader) As DateTimeOffset
         Try
            reader.MoveToContent()
            Return Common.CreateDateTimeOffset(reader.ReadElementString())
         Catch
         End Try
      End Function

      ''' <summary>
      ''' Evaluates the current node as a AtomLink-instance
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateLink(reader As sx.XmlReader) As AtomLink
         Return CreateLink(Of AtomLink)(reader,
                                        Function(href, relationshipType, mediaType, id, renditionKind)
                                           Return New AtomLink(New Uri(href), relationshipType, mediaType, id, renditionKind)
                                        End Function)
      End Function

      ''' <summary>
      ''' Evaluates the current node as a XElement-instance
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateLink(reader As sx.XmlReader, ns As sxl.XNamespace, elementName As String) As sxl.XElement
         With New XElementBuilder(ns, elementName)
            Return CreateLink(Of sxl.XElement)(reader, AddressOf .CreateXElement)
         End With
      End Function

      ''' <summary>
      ''' Creates a link
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function CreateLink(Of TResult)(reader As sx.XmlReader, factory As GenericDelegates(Of String, TResult).CreateLinkDelegate) As TResult
         Dim href As String = Nothing
         Dim relationshipType As String = Nothing
         Dim mediaType As String = Nothing
         Dim id As String = Nothing
         Dim renditionKind As String = Nothing
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         For index As Integer = 0 To reader.AttributeCount - 1
            reader.MoveToAttribute(index)
            Select Case reader.NamespaceURI
               Case Constants.Namespaces.atom, ""
                  Select Case reader.LocalName
                     Case "href", "src"
                        href = reader.GetAttribute(index)
                     Case "rel"
                        relationshipType = reader.GetAttribute(index)
                     Case "type"
                        mediaType = reader.GetAttribute(index)
                  End Select
               Case Constants.Namespaces.cmisra
                  Select Case reader.LocalName
                     Case "id"
                        id = reader.GetAttribute(index)
                     Case "renditionKind"
                        renditionKind = reader.GetAttribute(index)
                  End Select
            End Select
         Next
         'read to end of link
         reader.ReadStartElement()
         If Not isEmptyElement Then
            ReadToEndElement(reader, True)
         End If

         Return factory(href, relationshipType, mediaType, id, renditionKind)
      End Function

   End Module
End Namespace