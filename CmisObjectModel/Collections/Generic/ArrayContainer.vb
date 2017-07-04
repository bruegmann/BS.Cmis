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

Namespace CmisObjectModel.Collections.Generic
   ''' <summary>
   ''' Encapsulates an array and allows access to it via a DynamicProperty
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayContainer(Of T)
      Inherits Common.Generic.DynamicProperty(Of T())

      Public Sub New(propertyName As String, ParamArray values As T())
         MyBase.New(propertyName)
         _values = values
      End Sub

      Public Overrides ReadOnly Property CanRead As Boolean
         Get
            Return True
         End Get
      End Property

      Public Overrides ReadOnly Property CanWrite As Boolean
         Get
            Return True
         End Get
      End Property

      Protected _values As T()
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public Overrides Property Value As T()
         Get
            Return _values
         End Get
         Set(value As T())
            _values = value
         End Set
      End Property
      Public Property Values As T()
         Get
            Return _values
         End Get
         Set(value As T())
            _values = value
         End Set
      End Property

   End Class
End Namespace