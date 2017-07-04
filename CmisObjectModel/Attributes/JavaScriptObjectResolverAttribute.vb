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
   ''' Defines non standard instance factories for union constructs
   ''' </summary>
   ''' <remarks></remarks>
   Public Class JavaScriptObjectResolverAttribute
      Inherits Attribute

      Public Sub New(objectResolverType As Type)
         Me._objectResolverTypeDescriptor = DynamicTypeDescriptor.CreateInstance(objectResolverType)
      End Sub

      ''' <summary>
      ''' To define a generic union factory type
      ''' </summary>
      ''' <param name="objectResolverType"></param>
      ''' <param name="jsonFormattedGenericArgumentsMapping">Maps generic arguments of sourceType to
      ''' generic arguments of the genericTargetType in JSON format
      ''' i.e. {"TSourceTypeArgument3":"TTargetTypeArgument0","TSourceTypeArgument1":"TTargetTypeArgument1"}</param>
      ''' <remarks></remarks>
      Public Sub New(objectResolverType As Type,
                     jsonFormattedGenericArgumentsMapping As String)
         Me._objectResolverTypeDescriptor = DynamicTypeDescriptor.CreateInstance(objectResolverType,
                                                                                 jsonFormattedGenericArgumentsMapping)
      End Sub

      Public ReadOnly Property ObjectResolverType(sourceType As Type) As Type
         Get
            Return _objectResolverTypeDescriptor.CreateType(sourceType)
         End Get
      End Property

      Private ReadOnly _objectResolverTypeDescriptor As DynamicTypeDescriptor
   End Class
End Namespace