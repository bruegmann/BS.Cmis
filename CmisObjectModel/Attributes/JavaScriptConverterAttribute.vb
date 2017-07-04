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

Namespace CmisObjectModel.Attributes
   ''' <summary>
   ''' Defines the JavaScriptConverterType for a type
   ''' </summary>
   ''' <remarks></remarks>
   <AttributeUsage(AttributeTargets.Class)>
   Public Class JavaScriptConverterAttribute
      Inherits Attribute

      ''' <summary>
      ''' To define a converter type directly
      ''' </summary>
      ''' <param name="javaScriptConverterType"></param>
      ''' <remarks></remarks>
      Public Sub New(javaScriptConverterType As Type)
         _javaScriptConverterType = DynamicTypeDescriptor.CreateInstance(javaScriptConverterType)
      End Sub

      ''' <summary>
      ''' To define a generic converter type
      ''' </summary>
      ''' <param name="javaScriptConverterType"></param>
      ''' <param name="jsonFormattedGenericArgumentsMapping">Maps generic arguments of sourceType to
      ''' generic arguments of the genericTargetType in JSON format
      ''' i.e. {"TSourceTypeArgument3":"TTargetTypeArgument0","TSourceTypeArgument1":"TTargetTypeArgument1"}</param>
      ''' <remarks></remarks>
      Public Sub New(javaScriptConverterType As Type,
                     jsonFormattedGenericArgumentsMapping As String)
         _javaScriptConverterType = DynamicTypeDescriptor.CreateInstance(javaScriptConverterType, jsonFormattedGenericArgumentsMapping)
      End Sub

      Private _javaScriptConverterType As DynamicTypeDescriptor

      Public ReadOnly Property JavaScriptConverterType(sourceType As Type) As Type
         Get
            Return _javaScriptConverterType.CreateType(sourceType)
         End Get
      End Property
   End Class
End Namespace