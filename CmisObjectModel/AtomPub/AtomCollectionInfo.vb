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
   ''' Represents a collection of the cmis-repository workspace
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AtomCollectionInfo
      Inherits sss.ResourceCollectionInfo

#Region "Constants"
      Private Const csCollectionType As String = "collectionType"
#End Region

#Region "Constructors"
      Public Sub New()
         MyBase.New()

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Public Sub New(title As String, link As Uri, collectionType As String,
                     ParamArray accepts As String())
         Me.New()

         If title <> "" Then Me.Title = New sss.TextSyndicationContent(title)
         Me.Link = link
         Me.CollectionType = collectionType
         If accepts IsNot Nothing Then
            For Each accept As String In accepts
               Me.Accepts.Add(accept)
            Next
         End If
      End Sub

      ''' <summary>
      ''' Creates a new instance (similar to ReadXml() in IXmlSerializable-classes)
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As sx.XmlReader) As AtomCollectionInfo
         Dim accepts As New List(Of String)
         Dim collectionType As String = Nothing
         Dim link As Uri
         Dim title As String = Nothing
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         link = New Uri(reader.GetAttribute("href"))
         reader.ReadStartElement()
         If Not isEmptyElement Then
            reader.MoveToContent()
            While reader.IsStartElement
               Select Case reader.NamespaceURI
                  Case Constants.Namespaces.app
                     If reader.LocalName = "accept" Then
                        Dim accept As String = reader.ReadElementString()
                        If accept <> "" Then accepts.Add(accept)
                     Else
                        'ignore node
                        reader.ReadOuterXml()
                     End If
                  Case Constants.Namespaces.atom
                     If reader.LocalName = "title" Then
                        title = reader.ReadElementString()
                     Else
                        'ignore node
                        reader.ReadOuterXml()
                     End If
                  Case Constants.Namespaces.cmisra
                     If reader.LocalName = csCollectionType Then
                        collectionType = reader.ReadElementString()
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

         Return New AtomCollectionInfo(title, link, collectionType, If(accepts.Count > 0, accepts.ToArray(), Nothing))
      End Function
#End Region

#Region "AtomPub-extensions"
      Private _collectionType As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension CollectionType
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property CollectionType As String
         Get
            Return If(_collectionType Is Nothing, Nothing, _collectionType.GetObject(Of String))
         End Get
         Private Set(value As String)
            If _collectionType IsNot Nothing Then Me.ElementExtensions.Remove(_collectionType)
            If value Is Nothing Then
               _collectionType = Nothing
            Else
               _collectionType = New sss.SyndicationElementExtension(csCollectionType, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_collectionType)
            End If
         End Set
      End Property 'CollectionType
#End Region

   End Class
End Namespace