
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;

using Microsoft.EntityFrameworkCore;
using MauiAppCrud.DataAccess;
using MauiAppCrud.DTOs;
using MauiAppCrud.Utilidades;
using MauiAppCrud.Modelos;

namespace MauiAppCrud.ViewModels
{
    public partial class EmpleadoViewModel : ObservableObject, IQueryAttributable
    {
        private readonly EmpleadoDbContext _dbContext;

        [ObservableProperty]
        private EmpleadoDTO empleadoDto = new EmpleadoDTO();

        [ObservableProperty]
        private string tituloPagina;

        private int IdEmpleado;

        [ObservableProperty]
        private bool loadingEsVisible = false;

        public EmpleadoViewModel(EmpleadoDbContext context)
        {
            _dbContext = context;
            EmpleadoDto.FechaContrato = DateTime.Now;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var id = int.Parse(query["id"].ToString());
            IdEmpleado = id;

            if(IdEmpleado == 0)
            {
                TituloPagina = "Nuevo Empleado";
            }
            else
            {
                TituloPagina = "Editar Empleado";
                LoadingEsVisible = true;
                await Task.Run(async () =>
                {
                    var encontrado = await _dbContext.Empleados.FirstAsync(e => e.IdEmpleado == IdEmpleado);
                    EmpleadoDto.IdEmpleado = encontrado.IdEmpleado;
                    EmpleadoDto.NombreCompleto = encontrado.NombreCompleto;
                    EmpleadoDto.Correo = encontrado.Correo;
                    EmpleadoDto.Sueldo = encontrado.Sueldo;
                    EmpleadoDto.FechaContrato = encontrado.FechaContrato;

                    MainThread.BeginInvokeOnMainThread(() => { LoadingEsVisible = false; });
                });
            }
        }

        [RelayCommand]
        private async Task Guardar()
        {
            LoadingEsVisible = true;
            EmpleadoMensaje mensaje = new EmpleadoMensaje();

            await Task.Run(async () =>
            {
                if(IdEmpleado == 0)
                {
                    var tbEmpleado = new Empleado
                    {
                        NombreCompleto = EmpleadoDto.NombreCompleto,
                        Correo = EmpleadoDto.Correo,
                        Sueldo = EmpleadoDto.Sueldo,
                        FechaContrato = EmpleadoDto.FechaContrato,
                    };

                    _dbContext.Empleados.Add(tbEmpleado);
                    await _dbContext.SaveChangesAsync();

                    EmpleadoDto.IdEmpleado = tbEmpleado.IdEmpleado;
                    mensaje = new EmpleadoMensaje()
                    {
                        EsCrear = true,
                        EmpleadoDto = EmpleadoDto
                    };

                }
                else
                {
                    var encontrado = await _dbContext.Empleados.FirstAsync(e => e.IdEmpleado == IdEmpleado);
                    encontrado.NombreCompleto = EmpleadoDto.NombreCompleto;
                    encontrado.Correo = EmpleadoDto.Correo;
                    encontrado.Sueldo = EmpleadoDto.Sueldo;
                    encontrado.FechaContrato = EmpleadoDto.FechaContrato;

                    await _dbContext.SaveChangesAsync();

                    mensaje = new EmpleadoMensaje()
                    {
                        EsCrear = false,
                        EmpleadoDto = EmpleadoDto
                    };

                }

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    LoadingEsVisible = false;
                    WeakReferenceMessenger.Default.Send(new EmpleadoMensajeria(mensaje));
                    await Shell.Current.Navigation.PopAsync();
                });

            });
        }


    }
}
