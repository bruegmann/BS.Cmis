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
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client.Generic
   ''' <summary>
   ''' Generic container structure for types and objects in folders
   ''' </summary>
   ''' <typeparam name="TItem"></typeparam>
   ''' <remarks></remarks>
   Public Class ItemContainer(Of TItem)
      Implements IEnumerable(Of ItemContainer(Of TItem))

      Public Sub New(item As TItem)
         _item = item
      End Sub

      Private _children As New List(Of ItemContainer(Of TItem))
      Public ReadOnly Property Children As List(Of ItemContainer(Of TItem))
         Get
            Return _children
         End Get
      End Property

      ''' <summary>
      ''' Returns all items without any informations about the logical hierarchy of the items.
      ''' </summary>
      Public Function GetAllItems() As System.Collections.Generic.List(Of TItem)
         Dim retVal As New System.Collections.Generic.List(Of TItem)

         GetAllItems(retVal)
         Return retVal
      End Function
      Private Sub GetAllItems(values As System.Collections.Generic.List(Of TItem))
         values.Add(_item)
         For Each child As ItemContainer(Of TItem) In _children
            child.GetAllItems(values)
         Next
      End Sub
      Public Shared Function GetAllItems(itemContainers As IEnumerable(Of ItemContainer(Of TItem))) As System.Collections.Generic.List(Of TItem)
         Dim retVal As New System.Collections.Generic.List(Of TItem)

         If itemContainers IsNot Nothing Then
            For Each itemContainer As ItemContainer(Of TItem) In itemContainers
               If itemContainer IsNot Nothing Then itemContainer.GetAllItems(retVal)
            Next
         End If

         Return retVal
      End Function

      Private _item As TItem
      Public ReadOnly Property Item As TItem
         Get
            Return _item
         End Get
      End Property

      Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of ItemContainer(Of TItem)) Implements System.Collections.Generic.IEnumerable(Of ItemContainer(Of TItem)).GetEnumerator
         Return _children.GetEnumerator
      End Function

      Private Function IEnumerator_GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
         Return GetEnumerator()
      End Function
   End Class
End Namespace