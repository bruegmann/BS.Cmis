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
Imports cc = CmisObjectModel.Core
Imports ccc = CmisObjectModel.Core.Collections
Imports ccg = CmisObjectModel.Collections.Generic
Imports ccg1 = CmisObjectModel.Common.Generic
Imports ccp = CmisObjectModel.Core.Properties
Imports cjs = CmisObjectModel.JSON.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core.Collections
   Partial Class cmisPropertiesType
      Public ReadOnly Property DefaultArrayProperty As ccg1.DynamicProperty(Of ccp.cmisProperty())
         Get
            Return New ccg1.DynamicProperty(Of ccp.cmisProperty())(Function() _properties,
                                                                   Sub(value)
                                                                      Properties = value
                                                                   End Sub,
                                                                   "Properties")
         End Get
      End Property
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Collections
   ''' <summary>
   ''' Representation of cmisPropertiesType as a string-to-cmisProperty-map
   ''' </summary>
   ''' <remarks></remarks>
   Public Class Properties
      Inherits CmisObjectModel.Collections.Generic.ArrayMapper(Of CmisObjectModel.Core.Collections.cmisPropertiesType, 
                                                                  CmisObjectModel.Core.Properties.cmisProperty, String)

      Public Sub New(owner As CmisObjectModel.Core.Collections.cmisPropertiesType)
         MyBase.New(owner, owner.DefaultArrayProperty, ccp.cmisProperty.DefaultKeyProperty)
      End Sub

#Region "IJavaSerializationProvider"
      ''' <summary>
      ''' More comfortable access to JavaExport
      ''' </summary>
      Public Shadows Function JavaExport(serializer As cjs.JavaScriptSerializer) As IDictionary(Of String, IDictionary(Of String, Object))
         Return TryCast(MyBase.JavaExport(Me, serializer), IDictionary(Of String, IDictionary(Of String, Object)))
      End Function

      ''' <summary>
      ''' More comfortable access to JavaImport
      ''' </summary>
      Public Shadows Function JavaImport(source As IDictionary(Of String, IDictionary(Of String, Object)),
                                         serializer As cjs.JavaScriptSerializer) As CmisObjectModel.Core.Collections.cmisPropertiesType
         MyBase.JavaImport(source, serializer)
         Return _owner
      End Function
#End Region

   End Class
End Namespace