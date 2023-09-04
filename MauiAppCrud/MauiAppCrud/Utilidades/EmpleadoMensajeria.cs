using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MauiAppCrud.Utilidades
{
    public class EmpleadoMensajeria : ValueChangedMessage<EmpleadoMensaje>
    {
        public EmpleadoMensajeria(EmpleadoMensaje value) : base(value)
        {
            
        }
    }
}
