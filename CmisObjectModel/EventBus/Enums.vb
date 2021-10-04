Namespace CmisObjectModel.EventBus

   <Flags()>
   Public Enum enumBuiltInEvents As Integer
      BeginCancelCheckout = CancelCheckout Or enumBuiltInEventMasks.flgBegin
      BeginCheckIn = CheckIn Or enumBuiltInEventMasks.flgBegin
      BeginDeleteObject = DeleteObject Or enumBuiltInEventMasks.flgBegin

      CancelCheckout = 4
      CheckIn = 8
      DeleteObject = 16

      EndCancelCheckout = CancelCheckout Or enumBuiltInEventMasks.flgEnd
      EndCheckIn = CheckIn Or enumBuiltInEventMasks.flgEnd
      EndDeleteObject = DeleteObject Or enumBuiltInEventMasks.flgEnd
   End Enum

   <Flags()>
   Public Enum enumBuiltInEventMasks As Integer
      flgBegin = 1
      flgEnd = 2

      maskEventNames = enumBuiltInEvents.CancelCheckout Or enumBuiltInEvents.CheckIn Or enumBuiltInEvents.DeleteObject
      maskBeginOrEnd = flgBegin Or flgEnd
   End Enum

   Public Enum enumEventBusListenerResult As Integer
      success = 0
      removeListener = 1
      dontCare = 2
   End Enum
End Namespace