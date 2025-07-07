// Kuotasmig.Core/Services/CalculadoraService.cs

using Kuotasmig.Core.Data;
using Kuotasmig.Core.Models;
using Microsoft.EntityFrameworkCore; // Necesitamos este using para métodos como 'FirstOrDefault'
using System;
using System.Linq; // Necesitamos este using para métodos como 'OrderByDescending'

namespace Kuotasmig.Core.Services
{
    public class CalculadoraService
    {
        private readonly ApplicationDbContext _context;

        // El constructor sigue pidiendo el DbContext, ¡eso está perfecto!
        public CalculadoraService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public CalculadoraEntry? BuscarUltimo()
        {
            // Usamos LINQ para ordenar por ID y tomar el primero. ¡Mucho más limpio!
            return _context.Calculadora.OrderByDescending(c => c.ID).FirstOrDefault();
        }

        public int Agregar(CalculadoraEntry nuevaEntrada)
        {
            _context.Calculadora.Add(nuevaEntrada);
            return _context.SaveChanges(); // SaveChanges() guarda en la BD y devuelve las filas afectadas.
        }

        public int Modificar(CalculadoraEntry entradaAModificar)
        {
            _context.Calculadora.Update(entradaAModificar);
            return _context.SaveChanges();
        }

        public int EliminarPorId(int id)
        {
            var entradaParaEliminar = _context.Calculadora.Find(id);
            if (entradaParaEliminar != null)
            {
                _context.Calculadora.Remove(entradaParaEliminar);
                return _context.SaveChanges();
            }
            return 0; // No se encontró nada para eliminar
        }

        // Los otros métodos como BuscarPorId, ModificarTodo, y EliminarPorTexto
        // se pueden implementar de forma similar si los necesitas, pero estos
        // son los principales.
    }
}