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
   Public Class AtomWorkspace
      Inherits sss.Workspace

#Region "Constants"
      Private Const csLink As String = "link"
      Private Const csRepositoryInfo As String = "repositoryInfo"
      Private Const csUriTemplate As String = "uritemplate"
#End Region

#Region "Constructors"
      Public Sub New()
         MyBase.New()

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Public Sub New(title As String, repositoryInfo As Core.cmisRepositoryInfoType,
                     collections As List(Of AtomCollectionInfo),
                     links As List(Of sxl.XElement),
                     uriTemplates As List(Of RestAtom.cmisUriTemplateType))
         Me.New()

         If title <> "" Then Me.Title = New sss.TextSyndicationContent(title)
         If collections IsNot Nothing Then
            For Each collection As AtomCollectionInfo In collections
               'omit duplicate namespace definitions
               For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
                  collection.AttributeExtensions.Remove(de.Key)
               Next
               Me.Collections.Add(collection)
            Next
         End If
         Me.Links = If(links Is Nothing OrElse links.Count = 0, Nothing, links.ToArray())
         Me.RepositoryInfo = repositoryInfo
         Me.UriTemplates = If(uriTemplates Is Nothing OrElse uriTemplates.Count = 0, Nothing, uriTemplates.ToArray())
      End Sub

      ''' <summary>
      ''' Creates a new instance (similar to ReadXml() in IXmlSerializable-classes)
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As sx.XmlReader) As AtomWorkspace
         Dim collections As New List(Of AtomCollectionInfo)
         Dim links As New List(Of sxl.XElement)
         Dim repositoryInfo As Core.cmisRepositoryInfoType = Nothing
         Dim title As String = Nothing
         Dim uriTemplates As New List(Of RestAtom.cmisUriTemplateType)
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         reader.ReadStartElement()
         If Not isEmptyElement Then
            reader.MoveToContent()
            While reader.IsStartElement
               Select Case reader.NamespaceURI
                  Case Constants.Namespaces.app
                     If reader.LocalName = "collection" Then
                        collections.Add(AtomCollectionInfo.CreateInstance(reader))
                     Else
                        'ignore node
                        reader.ReadOuterXml()
                     End If
                  Case Constants.Namespaces.atom
                     Select Case reader.LocalName
                        Case "link"
                           links.Add(Factory.CreateLink(reader, Constants.Namespaces.atom, "link"))
                        Case "title"
                           title = reader.ReadElementString()
                        Case Else
                           'ignore node
                           reader.ReadOuterXml()
                     End Select
                  Case Constants.Namespaces.cmisra
                     Select Case reader.LocalName
                        Case csRepositoryInfo
                           repositoryInfo = New Core.cmisRepositoryInfoType
                           repositoryInfo.ReadXml(reader)
                        Case csUriTemplate
                           Dim uriTemplate As New RestAtom.cmisUriTemplateType
                           uriTemplate.ReadXml(reader)
                           uriTemplates.Add(uriTemplate)
                        Case Else
                           'ignore node
                           reader.ReadOuterXml()
                     End Select
                  Case Else
                     'ignore node
                     reader.ReadOuterXml()
               End Select
               reader.MoveToContent()
            End While

            reader.ReadEndElement()
         End If

         Return New AtomWorkspace(title, repositoryInfo, collections, links, uriTemplates)
      End Function
#End Region

#Region "AtomPub-extensions"
      Private _links As List(Of sss.SyndicationElementExtension)
      ''' <summary>
      ''' AtomPub extension Links
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property Links As sxl.XElement()
         Get
            Return If(_links Is Nothing, Nothing,
                      (From link As sss.SyndicationElementExtension In _links
                       Select link.GetObject(Of sxl.XElement)()).ToArray())
         End Get
         Private Set(value As sxl.XElement())
            'discard quick access
            _linkCache = Nothing

            If _links IsNot Nothing Then
               For Each link As sss.SyndicationElementExtension In _links
                  Me.ElementExtensions.Remove(link)
               Next
            End If
            If value Is Nothing Then
               _links = Nothing
            Else
               _links = New List(Of sss.SyndicationElementExtension)
               For Each link As sxl.XElement In value
                  Dim linkExtension As New sss.SyndicationElementExtension(link)
                  _links.Add(linkExtension)
                  Me.ElementExtensions.Add(linkExtension)
               Next
            End If
         End Set
      End Property 'Links

      Private _repositoryInfo As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension RepositoryInfo
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property RepositoryInfo As Core.cmisRepositoryInfoType
         Get
            Return If(_repositoryInfo Is Nothing, Nothing, _repositoryInfo.GetObject(Of Core.cmisRepositoryInfoType))
         End Get
         Private Set(value As Core.cmisRepositoryInfoType)
            If _repositoryInfo IsNot Nothing Then Me.ElementExtensions.Remove(_repositoryInfo)
            If value Is Nothing Then
               _repositoryInfo = Nothing
            Else
               _repositoryInfo = New sss.SyndicationElementExtension(csRepositoryInfo, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_repositoryInfo)
            End If
         End Set
      End Property 'RepositoryInfo

      Private _uriTemplates As List(Of sss.SyndicationElementExtension)
      ''' <summary>
      ''' AtomPub extension UriTemplates
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property UriTemplates As RestAtom.cmisUriTemplateType()
         Get
            Return If(_uriTemplates Is Nothing, Nothing,
                      (From uriTemplate As sss.SyndicationElementExtension In _uriTemplates
                       Select uriTemplate.GetObject(Of RestAtom.cmisUriTemplateType)()).ToArray())
         End Get
         Private Set(value As RestAtom.cmisUriTemplateType())
            'discard quick access
            _uriTemplateCache = Nothing

            If _uriTemplates IsNot Nothing Then
               For Each uriTemplate As sss.SyndicationElementExtension In _uriTemplates
                  Me.ElementExtensions.Remove(uriTemplate)
               Next
            End If
            If value Is Nothing Then
               _uriTemplates = Nothing
            Else
               _uriTemplates = New List(Of sss.SyndicationElementExtension)
               For Each uriTemplate As RestAtom.cmisUriTemplateType In value
                  Dim uriTemplateExtension As New sss.SyndicationElementExtension(csUriTemplate, Constants.Namespaces.cmisra, uriTemplate)
                  _uriTemplates.Add(uriTemplateExtension)
                  Me.ElementExtensions.Add(uriTemplateExtension)
               Next
            End If
         End Set
      End Property 'UriTemplates
#End Region

#Region "quick access to specific collectionInfo, repositoryLink or uriTemplate"
      Private _collectionInfoCache As Collections.Generic.Cache(Of String, AtomCollectionInfo)
      ''' <summary>
      ''' Quick access to CollectionInfo
      ''' </summary>
      ''' <param name="collectionType"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property CollectionInfo(collectionType As String) As AtomCollectionInfo
         Get
            SyncLock _syncObject
               Dim repositoryId As String = If(Me.RepositoryInfo.RepositoryId, "")

               If _collectionInfoCache Is Nothing Then
                  _collectionInfoCache = New Collections.Generic.Cache(Of String, AtomCollectionInfo)(1000, Double.PositiveInfinity, False)
                  For Each collection As sss.ResourceCollectionInfo In Me.Collections
                     Try
                        Dim atomCollection As AtomCollectionInfo = TryCast(collection, AtomCollectionInfo)

                        If atomCollection IsNot Nothing Then
                           _collectionInfoCache.Item(repositoryId, If(atomCollection.CollectionType, "")) = atomCollection
                        End If
                     Catch
                     End Try
                  Next
               End If

               Return _collectionInfoCache.Item(repositoryId, If(collectionType, ""))
            End SyncLock
         End Get
      End Property

      Private _linkCache As Collections.Generic.Cache(Of String, String)
      ''' <summary>
      ''' Quick access to specified link
      ''' </summary>
      ''' <param name="relationshipType"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Link(relationshipType As String) As String
         Get
            SyncLock _syncObject
               Dim repositoryId As String = If(Me.RepositoryInfo.RepositoryId, "")

               'build the cache
               If _linkCache Is Nothing Then
                  Dim links As sxl.XElement() = Me.Links

                  _linkCache = New Collections.Generic.Cache(Of String, String)(1000, Double.PositiveInfinity, False)
                  If links IsNot Nothing Then
                     For Each item As sx.Linq.XElement In links
                        Try
                           _linkCache.Item(repositoryId, If(item.Attribute("rel").Value, "")) = item.Attribute("href").Value
                        Catch
                        End Try
                     Next
                  End If
               End If

                  Return _linkCache.Item(repositoryId, If(relationshipType, ""))
            End SyncLock
         End Get
      End Property

      Private _uriTemplateCache As Collections.Generic.Cache(Of String, RestAtom.cmisUriTemplateType)
      ''' <summary>
      ''' Quick access to UriTemplate
      ''' </summary>
      ''' <param name="templateType"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property UriTemplate(templateType As String) As RestAtom.cmisUriTemplateType
         Get
            SyncLock _syncObject
               Dim repositoryId As String = If(Me.RepositoryInfo.RepositoryId, "")

               'build the cache
               If _uriTemplateCache Is Nothing Then
                  Dim uriTemplates As RestAtom.cmisUriTemplateType() = Me.UriTemplates

                  _uriTemplateCache = New Collections.Generic.Cache(Of String, RestAtom.cmisUriTemplateType)(1000, Double.PositiveInfinity, False)
                  If uriTemplates IsNot Nothing Then
                     For Each item As RestAtom.cmisUriTemplateType In uriTemplates
                        'Using Alfresco the parameter returnVersion is missed in template ObjectById
                        If If(item.Type, "").ToLowerInvariant() = "objectbyid" Then
                           Try
                              Dim ut As New UriTemplate(item.Template)
                              Dim cmisDefinedParameter As New Dictionary(Of String, String) From {{"id", "id"}, {"filter", "filter"},
                                                                                                  {"includeallowableactions", "includeAllowableActions"},
                                                                                                  {"includepolicyids", "includePolicyIds"},
                                                                                                  {"includerelationships", "includeRelationships"},
                                                                                                  {"includeacl", "includeACL"},
                                                                                                  {"renditionfilter", "renditionFilter"},
                                                                                                  {"returnversion", "returnVersion"}}
                              Dim missedParameters As String() = cmisDefinedParameter.Keys.Except(From queryValueVariableName As String In ut.QueryValueVariableNames
                                                                                                  Select queryValueVariableName.ToLowerInvariant()).ToArray
                              If missedParameters.Length > 0 Then
                                 item.Template &= If(ut.QueryValueVariableNames.Count = 0, "?", "&") & String.Join("&", (From missedParameterName As String In missedParameters
                                                                                                                         Select cmisDefinedParameter(missedParameterName) & "={" & cmisDefinedParameter(missedParameterName) & "}").ToArray())
                              End If
                           Catch
                           End Try
                        End If
                        _uriTemplateCache.Item(repositoryId, If(item.Type, "")) = item
                     Next
                  End If
               End If

               Return _uriTemplateCache.Item(repositoryId, If(templateType, ""))
            End SyncLock
         End Get
      End Property
#End Region

      Private _syncObject As New Object

   End Class
End Namespace