namespace Helpers
{
    public static class Convertidor
    {
        public static TModeloEntrada Clonar<TModeloEntrada>(TModeloEntrada elemento) where TModeloEntrada : class, new() => Convertidor.Convertir<TModeloEntrada, TModeloEntrada>(elemento);

        public static TModeloSalida Convertir<TModeloEntrada, TModeloSalida>(
          TModeloEntrada elementoBase,
          Action<TModeloEntrada, TModeloSalida> reglaTransformacion = null)
          where TModeloEntrada : class
          where TModeloSalida : new()
        {
            return Mapeador.MapearObjetos<TModeloEntrada, TModeloSalida>(elementoBase, reglaTransformacion);
        }

        public static IEnumerable<TModeloSalida> ConvertirLista<TModeloEntrada, TModeloSalida>(
          IEnumerable<TModeloEntrada> elementos,
          Action<TModeloEntrada, TModeloSalida> reglaTransformacion = null)
          where TModeloEntrada : class
          where TModeloSalida : new()
        {
            return (IEnumerable<TModeloSalida>)elementos.Select<TModeloEntrada, TModeloSalida>((Func<TModeloEntrada, TModeloSalida>)(elementoBase => Mapeador.MapearObjetos<TModeloEntrada, TModeloSalida>(elementoBase, reglaTransformacion))).ToList<TModeloSalida>();
        }
    }
}
