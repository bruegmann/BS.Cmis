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
Imports ccc = CmisObjectModel.Core.Collections
Imports ccg = CmisObjectModel.Collections.Generic
Imports ccg1 = CmisObjectModel.Common.Generic
Imports ccp = CmisObjectModel.Core.Properties
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Collections
   ''' <summary>
   ''' Converter for the cmisPropertiesType when requested as succinctProperties-collection
   ''' </summary>
   ''' <remarks></remarks>
   Public Class SuccinctPropertiesConverter
      Inherits JSON.Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Core.Collections.cmisPropertiesType)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Core.Collections.cmisPropertiesType))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Core.Collections.cmisPropertiesType))
         MyBase.New(objectResolver)
      End Sub
#End Region

      ''' <summary>
      ''' Deserializes the cmisPropertiesType from a string-to-object-map
      ''' </summary>
      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         Dim map As New SuccinctProperties(context.Object)
         map.JavaImport(context.Dictionary)
      End Sub

      ''' <summary>
      ''' Serializes the cmisPropertiesType as a string-to-object-map
      ''' </summary>
      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         Dim map As New SuccinctProperties(context.Object)
         Dim data As IDictionary(Of String, Object) = map.JavaExport()

         If data IsNot Nothing Then
            For Each de As KeyValuePair(Of String, Object) In data
               context.Add(de.Key, de.Value)
            Next
         End If
      End Sub
   End Class
End Namespace