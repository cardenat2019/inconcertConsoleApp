using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Utils
{
    static class Constants
    {
        public const string TipoStatus_1_Contacto    = "Contacto";
        public const string TipoStatus_1_NoContacto  = "No Contacto";
        public const string TipoStatus_1_Contactado  = "Contactado";
        public const string TipoStatus_2_Reprogramar = "Reprogramar";
        public const string TipoStatus_2_NoDefine    = "No define";
        public const string TipoStatus_2_Negativa    = "Negativa";
        public const string TipoStatus_2_Salida      = "Salida";
        public const string TipoStatus_2_MensajeConTerceros = "Mensaje con Terceros";
        public const string Tipificacion_BuzonDeVoz = "Buzón de Voz";
        public const string Tipificacion_ClienteContestaYCuelga = "Cliente contesta y cuelga";
        public const string Tipificacion_NumeroSuspendidoFueraServicio = "Número suspendido / Fuera de servicio";
        public const string Tipificacion_SeCaeLlamadaEnMarcacion = "Se cae la llamada en marcación";
        public const string Tipificacion_TelefonoOcupado = "Teléfono Ocupado";
        public const string Tipificacion_NumeroEquivocado = "Número equivocado";
        public const string Tipificacion_Contactado = "Contactado";
        public const string Tipificacion_Cita = "Cita";
        public const string Tipificacion_ClientePideVolverALlamar = "Cliente pide volver a llamar";
        public const string Tipificacion_ProblemasAudioEnLLamada = "Problemas de audio durante la llamada";
        public const string Tipificacion_NoSeQueEstudiar = "No sé qué estudiar";
        public const string Tipificacion_NoEstoyListoVoyAPensar = "No estoy listo / Lo voy a pensar";
        public const string Tipificacion_RevisandoConPapasOfamiliares = "Revisando con papás o familiares";
        public const string Tipificacion_EnEsperaResultados = "En espera de resultados de la pública";
        public const string Tipificacion_EvaluandoOtrasUniversidadesProgramas = "Evaluando otras universidades/programas";
        public const string Tipificacion_EvaluandoTemasEconomicos = "Evaluando por temas económicos";
        public const string Tipificacion_CambioCiclo = "Cambio de ciclo";
        public const string Tipificacion_LejaniaCampus = "Lejanía del campus";
        public const string Tipificacion_NoSolicitoInformacion = "No solicitó información";
        public const string Tipificacion_HorariosClasesNoCompatibles = "Horarios de clases no compatibles";
        public const string Tipificacion_NoExisteProgramaEnCampus = "No existe el programa en el Campus";
        public const string Tipificacion_NoExisteOfertaAcademicaAliat = "No existe oferta académica en Aliat";
        public const string Tipificacion_InscritoEscuelaPrivada = "Inscrito en escuela privada";
        public const string Tipificacion_InscritoEscuelaPublica = "Inscrito en escuela pública";
        public const string Tipificacion_TemasEconomicos = "Temas económicos";
        public const string Tipificacion_ClientePideSacarMarcacion = "Cliente pide sacar de marcación";
        public const string Tipificacion_InscritoAliat = "Inscrito en Aliat";
        public const string Tipificacion_NoCubrePerfilEdadGrado = "No cubre perfil/edad/grado";
        public const string Tipificacion_SinContactoConInteresado = "Sin contacto con interesado";
        

        // Constantes para Entidades de Datos
        public const string Tipification_Rules     = "1";
        public const string Tipification_Variables = "2";
        public const string Ambientes = "3";
        public const string ParametrosSMTP = "4";
        public const string ParametrosServicios = "5";

        // Constante para definir Ambiente del SMTP Server
        public const string EnvironmentId = "30";

        // Intentos de enviar datos
        public const int RetriesSendingData = 2;

    }
}
