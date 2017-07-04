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
Namespace CmisObjectModel.ServiceModel
   Public Class cmisObjectParentsType
      Inherits Messaging.cmisObjectParentsType
      Implements Contracts.IServiceModelObject

      Public Shadows Property [Object] As cmisObjectType
         Get
            Return TryCast(_object, cmisObjectType)
         End Get
         Set(value As cmisObjectType)
            MyBase.Object = value
         End Set
      End Property

#Region "IServiceModelObject"
      Private ReadOnly Property IServiceModelObject_Object As cmisObjectType Implements Contracts.IServiceModelObject.Object
         Get
            Return Me.Object
         End Get
      End Property

      Public ReadOnly Property PathSegment As String Implements Contracts.IServiceModelObject.PathSegment
         Get
            Return Nothing
         End Get
      End Property

      Private ReadOnly Property IServiceModelObject_RelativePathSegment As String Implements Contracts.IServiceModelObject.RelativePathSegment
         Get
            Return _relativePathSegment
         End Get
      End Property

      Public ReadOnly Property ServiceModel As cmisObjectType.ServiceModelExtension Implements Contracts.IServiceModelObject.ServiceModel
         Get
            Dim cmisObject As cmisObjectType = Me.Object
            Return If(If(cmisObject Is Nothing, Nothing, cmisObject.ServiceModel), New cmisObjectType.ServiceModelExtension(cmisObject))
         End Get
      End Property
#End Region

   End Class
End Namespace