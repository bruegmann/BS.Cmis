'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2017, Brügmann Software GmbH, Papenburg, All rights reserved
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
Namespace CmisObjectModel.EventBus
   <HideModuleName()>
   Public Module BuiltInEventNames
      Public Const BeginCancelCheckout As String = "BeginCancelCheckout"
      Public Const BeginCheckIn As String = "BeginCheckIn"
      Public Const BeginDeleteObject As String = "BeginDeleteObject"
      Public Const EndCancelCheckout As String = "EndCancelCheckout"
      Public Const EndCheckIn As String = "EndCheckedIn"
      Public Const EndDeleteObject As String = "EndDeleteObject"

      Public Function GetBeginEventName(builtInEvent As enumBuiltInEvents) As String
         Return GetBeginOrEndEventName(builtInEvent, True)
      End Function

      Public Function GetEndEventName(builtInEvent As enumBuiltInEvents) As String
         Return GetBeginOrEndEventName(builtInEvent, False)
      End Function

      Private Function GetBeginOrEndEventName(builtInEvent As enumBuiltInEvents, flgBegin As Boolean) As String
         Dim flgBeginOrEnd As enumBuiltInEventMasks = If(flgBegin, enumBuiltInEventMasks.flgEnd, enumBuiltInEventMasks.flgBegin)
         Return GetEventName(CType((builtInEvent Or enumBuiltInEventMasks.maskBeginOrEnd) Xor flgBeginOrEnd, enumBuiltInEvents))
      End Function

      Public Function GetEventName(builtInEvent As enumBuiltInEvents) As String
         Dim flgBegin As Boolean = (builtInEvent And enumBuiltInEventMasks.flgBegin) = enumBuiltInEventMasks.flgBegin
         Dim flgEnd As Boolean = (builtInEvent And enumBuiltInEventMasks.flgEnd) = enumBuiltInEventMasks.flgEnd
         Dim eventName As enumBuiltInEvents = CType(builtInEvent And enumBuiltInEventMasks.maskEventNames, enumBuiltInEvents)

         Return If(flgBegin, "Begin", Nothing) & If(flgEnd, "End", Nothing) & eventName.ToString()
      End Function
   End Module
End Namespace