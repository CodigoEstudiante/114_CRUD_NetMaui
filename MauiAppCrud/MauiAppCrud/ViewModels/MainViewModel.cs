using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;

using Microsoft.EntityFrameworkCore;
using MauiAppCrud.DataAccess;
using MauiAppCrud.DTOs;
using MauiAppCrud.Utilidades;
using MauiAppCrud.Modelos;
using System.Collections.ObjectModel;
using MauiAppCrud.Views;

namespace MauiAppCrud.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly EmpleadoDbContext _dbContext;
        [ObservableProperty]
        private ObservableCollection<EmpleadoDTO> listaEmpleado = new ObservableCollection<EmpleadoDTO>();

        public MainViewModel(EmpleadoDbContext context)
        {
            _dbContext = context;

            MainThread.BeginInvokeOnMainThread(new Action(async () => await Obtener()));

            WeakReferenceMessenger.Default.Register<EmpleadoMensajeria>(this, (r, m) =>
            {
                EmpleadoMensajeRecibido(m.Value);
            });
        }

        public async Task Obtener()
        {
            var lista = await _dbContext.Empleados.ToListAsync();
            if(lista.Any())
            {
                foreach(var item in lista)
                {
                    ListaEmpleado.Add(new EmpleadoDTO
                    {
                        IdEmpleado = item.IdEmpleado,
                        NombreCompleto = item.NombreCompleto,
                        Correo = item.Correo,
                        Sueldo = item.Sueldo,
                        FechaContrato = item.FechaContrato,
                    });
                }
            }
        }

        private void EmpleadoMensajeRecibido(EmpleadoMensaje empleadoMensaje)
        {
            var empleadoDto = empleadoMensaje.EmpleadoDto;

            if (empleadoMensaje.EsCrear)
            {
                ListaEmpleado.Add(empleadoDto);
            }
            else
            {
                var encontrado = ListaEmpleado
                    .First(e => e.IdEmpleado ==  empleadoDto.IdEmpleado);

                encontrado.NombreCompleto = empleadoDto.NombreCompleto;
                encontrado.Correo = empleadoDto.Correo;
                encontrado.Sueldo = empleadoDto.Sueldo;
                encontrado.FechaContrato = empleadoDto.FechaContrato;

            }

        }

        [RelayCommand]
        private async Task Crear()
        {
            var uri = $"{nameof(EmpleadoPage)}?id=0";
            await Shell.Current.GoToAsync(uri);
        }

        [RelayCommand]
        private async Task Editar(EmpleadoDTO empleadoDto)
        {
            var uri = $"{nameof(EmpleadoPage)}?id={empleadoDto.IdEmpleado}";
            await Shell.Current.GoToAsync(uri);
        }

        [RelayCommand]
        private async Task Eliminar(EmpleadoDTO empleadoDto)
        {
            bool answer = await Shell.Current.DisplayAlert("Mensaje", "Desea eliminar el empleado?", "Si", "No");

            if (answer)
            {
                var encontrado = await _dbContext.Empleados
                    .FirstAsync(e => e.IdEmpleado == empleadoDto.IdEmpleado);

                _dbContext.Empleados.Remove(encontrado);
                await _dbContext.SaveChangesAsync();
                ListaEmpleado.Remove(empleadoDto);

            }

        }


    }
}
