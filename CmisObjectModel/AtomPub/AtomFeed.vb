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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.AtomPub
   ''' <summary>
   ''' Represents a list of child elements
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AtomFeed
      Inherits sss.SyndicationFeed

#Region "Constants"
      Private Const csHasMoreItems As String = "hasMoreItems"
      Private Const csNumItems As String = "numItems"
#End Region

#Region "Constructors"
      Public Sub New()
         MyBase.New()

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Public Sub New(parent As AtomEntry, entries As List(Of AtomEntry),
                     hasMoreItems As Boolean, numItems As xs_Integer?,
                     links As List(Of AtomLink))
         Me.New(parent.Id, If(parent.Title Is Nothing, Nothing, parent.Title.Text), parent.LastUpdatedTime,
                entries, hasMoreItems, numItems, links, parent.Authors.ToArray())
      End Sub

      Public Sub New(id As String, title As String, lastUpdatedTime As DateTimeOffset,
                     entries As List(Of AtomEntry), hasMoreItems As Boolean, numItems As xs_Integer?,
                     links As List(Of AtomLink),
                     ParamArray authors As sss.SyndicationPerson())
         Me.New()

         'conform to guideline (see 3.5.1 Feeds)
         'atom:updated SHOULD be the latest time the folder or its contents was updated. If unknown by the underlying repository, it MUST be the current time
         If lastUpdatedTime = DateTimeOffset.MinValue Then lastUpdatedTime = DateTimeOffset.UtcNow
         Me.Id = id
         Me.Title = New sss.TextSyndicationContent(title)
         Me.LastUpdatedTime = lastUpdatedTime
         Me.ElementExtensions.Add("edited", Constants.Namespaces.app, sx.XmlConvert.ToString(lastUpdatedTime))
         If authors IsNot Nothing Then
            For Each author As sss.SyndicationPerson In authors
               Me.Authors.Add(author)
            Next
         End If
         If links IsNot Nothing Then
            For Each link As AtomLink In links
               Me.Links.Add(link)
            Next
         End If
         If entries IsNot Nothing Then
            'omit duplicate namespace definitions
            For Each entry As AtomEntry In entries
               For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
                  entry.AttributeExtensions.Remove(de.Key)
               Next
            Next
            Me.Items = entries
         End If
         Me.HasMoreItems = hasMoreItems
         Me.NumItems = numItems
      End Sub

      ''' <summary>
      ''' Creates a new instance (similar to ReadXml() in IXmlSerializable-classes)
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As sx.XmlReader) As AtomFeed
         Dim authors As New List(Of sss.SyndicationPerson)
         Dim hasMoreItems As Boolean = False
         Dim entries As New List(Of AtomEntry)
         Dim id As String = Nothing
         Dim lastUpdatedTime As DateTimeOffset
         Dim links As New List(Of AtomLink)
         Dim numItems As xs_Integer? = Nothing
         Dim title As String = Nothing
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         reader.ReadStartElement()
         If Not isEmptyElement Then
            reader.MoveToContent()
            While reader.IsStartElement
               Select Case reader.NamespaceURI
                  Case Constants.Namespaces.atom
                     Select Case reader.LocalName
                        Case "author", "contributor"
                           authors.Add(Factory.CreateAuthor(reader))
                        Case "entry"
                           entries.Add(AtomEntry.CreateInstance(reader))
                        Case "id"
                           id = reader.ReadElementString()
                        Case "link"
                           links.Add(Factory.CreateLink(reader))
                        Case "title"
                           title = reader.ReadElementString()
                        Case "updated"
                           lastUpdatedTime = Factory.CreateDateTimeOffset(reader)
                        Case Else
                           'ignore node
                           reader.ReadOuterXml()
                     End Select
                  Case Constants.Namespaces.cmisra
                     Select Case reader.LocalName
                        Case csHasMoreItems
                           Dim boolValue As Boolean
                           hasMoreItems = Boolean.TryParse(reader.ReadElementString(), boolValue) AndAlso boolValue
                        Case csNumItems
                           Dim intValue As xs_Integer
                           If xs_Integer.TryParse(reader.ReadElementString(), intValue) Then
                              numItems = intValue
                           End If
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

         Return New AtomFeed(id, title, lastUpdatedTime,
                             If(entries.Count > 0, entries, Nothing),
                             hasMoreItems, numItems,
                             If(links.Count > 0, links, Nothing),
                             If(authors.Count > 0, authors.ToArray(), Nothing))
      End Function
#End Region

#Region "AtomPub-extensions"
      Private _hasMoreItems As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension HasMoreItems
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property HasMoreItems As Boolean
         Get
            Return _hasMoreItems IsNot Nothing AndAlso _hasMoreItems.GetObject(Of Boolean)()
         End Get
         Private Set(value As Boolean)
            If _hasMoreItems IsNot Nothing Then Me.ElementExtensions.Remove(_hasMoreItems)
            _hasMoreItems = New sss.SyndicationElementExtension(csHasMoreItems, Constants.Namespaces.cmisra, value)
            Me.ElementExtensions.Add(_hasMoreItems)
         End Set
      End Property 'HasMoreItems

      Private _numItems As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension NumItems
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property NumItems As xs_Integer?
         Get
            If _numItems Is Nothing Then
               Return Nothing
            Else
               Return _numItems.GetObject(Of xs_Integer)()
            End If
         End Get
         Private Set(value As xs_Integer?)
            If _numItems IsNot Nothing Then Me.ElementExtensions.Remove(_numItems)
            If value.HasValue Then
               _numItems = New sss.SyndicationElementExtension(csNumItems, Constants.Namespaces.cmisra, value.Value)
            Else
               _numItems = Nothing
            End If
         End Set
      End Property 'NumItems
#End Region

      <sxs.XmlIgnore()>
      Public ReadOnly Property Entries As List(Of AtomEntry)
         Get
            If Items Is Nothing Then
               Return Nothing
            ElseIf TypeOf Items Is List(Of AtomEntry) Then
               Return CType(Items, List(Of AtomEntry))
            Else
               Return (From item As sss.SyndicationItem In Items
                       Where TypeOf item Is AtomEntry
                       Select CType(item, AtomEntry)).ToList()
            End If
         End Get
      End Property

      ''' <summary>
      ''' Returns the first AtomEntry if objectId is not specified (null or empty), otherwise
      ''' the first AtomEntry-instance matching the specified objectId
      ''' </summary>
      ''' <param name="objectId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public ReadOnly Property Entry(objectId As String) As AtomEntry
         Get
            If Items IsNot Nothing Then
               For Each item As sss.SyndicationItem In Items
                  If TypeOf item Is AtomEntry Then
                     Dim retVal As AtomEntry = CType(item, AtomEntry)

                     If (String.IsNullOrEmpty(objectId) OrElse String.Compare(objectId, If(retVal.ObjectId, "")) = 0) Then
                        Return retVal
                     End If
                  End If
               Next
            End If

            'not found
            Return Nothing
         End Get
      End Property

      ''' <summary>
      ''' Returns the number of items of this feed
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>if numitems is set to -1 then the number of items in the item-collection is returned</remarks>
      Public Function GetNumItems() As xs_Integer?
         Dim retVal As xs_Integer? = Me.NumItems

         If retVal.HasValue AndAlso retVal.Value = -1 Then
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
            Return Items.Count()
#Else
            Return Items.LongCount()
#End If
         Else
            Return retVal
         End If
      End Function

   End Class
End Namespace