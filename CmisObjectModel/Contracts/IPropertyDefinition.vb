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
Imports cc = CmisObjectModel.Core
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccg = CmisObjectModel.Collections.Generic

Namespace CmisObjectModel.Contracts
   Public Interface IPropertyDefinition
      Property Cardinality As Core.enumCardinality
      Property Choices As Core.Choices.cmisChoice()
      ReadOnly Property ChoicesAsReadOnly As ccg.ArrayMapper(Of ccdp.cmisPropertyDefinitionType, cc.Choices.cmisChoice)
      ReadOnly Property ChoiceType As Type
      Function CreateProperty() As Core.Properties.cmisProperty
      Function CreateProperty(ParamArray values As Object()) As Core.Properties.cmisProperty
      ReadOnly Property CreatePropertyResultType As Type
      Property DefaultValue As Core.Properties.cmisProperty
      Property Inherited As Boolean?
      Property OpenChoice As Boolean?
      Property Orderable As Boolean
      ReadOnly Property PropertyType As Core.enumPropertyType
      ReadOnly Property PropertyValueType As Type
      Property Required As Boolean
      Property Updatability As Core.enumUpdatability
   End Interface
End Namespace