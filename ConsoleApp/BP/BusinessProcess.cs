using ConsoleApp.Models;
using ConsoleApp.Utils;
using ConsoleApp.Mail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp.BP
{
    static class BusinessProcess
    {
        // Método principal para ejecutar el enviar la info de tipificación al servicio del cliente 
        public static void executeProcessClientData()
        {
            List<ClientData> clientDataList = getClientData();
            foreach (var clientData in clientDataList)
            {
                List<ValoresResultados> valoresResultados = getValoresResultados(clientData);
                BaseDataPostMerge dataPostMerge = validacionesValoresResultados(valoresResultados, clientData);
                if (dataPostMerge.baseDataPosts.Count > 0)
                {
                    apiPost(dataPostMerge.baseDataPosts, dataPostMerge.baseDataPostContacts, clientData, valoresResultados).GetAwaiter().GetResult();
                    // Envio de correos
                }
            }
        }
        // Método que trae los datos del cliente VCC, Campaña
        public static List<ClientData> getClientData()
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            List<ClientData> clientDataList = new List<ClientData>();

            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spTipificationProcess", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ClientData clientData = new ClientData();

                        clientData.Id = Int32.Parse(reader[0].ToString());
                        clientData.VCC = reader[1].ToString();
                        clientData.Campaign = reader[2].ToString();
                        clientData.StartDateTime = reader[3].ToString();
                        clientData.EndDateTime = reader[4].ToString();
                        //clientData.StartDateTime = Convert.ToDateTime(reader[3].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        //clientData.EndDateTime = Convert.ToDateTime(reader[4].ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                        clientDataList.Add(clientData);
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error trayendo registro de TipificacionProcess", ex);
                addLogErrors("getClientData", ex.Message, null, null, null);
                //throw;
            }

            return clientDataList;
        }
        // Método para traer los datos por cada Entidad de Datos
        public static List<EntityValue> getEntityValues(string entityId)
        {
            List<EntityValue> entityValueList = new List<EntityValue>();

            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spEntityValues", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EntityId", entityId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        EntityValue entityValue = new EntityValue();

                        entityValue.Id = Int32.Parse(reader[0].ToString());
                        entityValue.EntityId = Int32.Parse(reader[1].ToString());
                        entityValue.Code = reader[2].ToString();
                        entityValue.Name = reader[3].ToString();
                        entityValue.Estatus = Boolean.Parse(reader[4].ToString()) ? 1 : 0;

                        entityValueList.Add(entityValue);
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error trayendo registro de Entidades de Datos", ex);
                addLogErrors("getEntityValues", ex.Message);
                //throw;
            }

            return entityValueList;
        }
        // Método que trae los resultados según los Datos del Cliente
        public static List<ValoresResultados> getValoresResultados(ClientData clientData)
        {
            List<ValoresResultados> valoresResultados = new List<ValoresResultados>();
            SqlDataReader reader = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spTipificacion", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VCC", clientData.VCC);
                    cmd.Parameters.AddWithValue("@Campaign", clientData.Campaign);
                    cmd.Parameters.AddWithValue("@StartDateTime", clientData.StartDateTime);
                    cmd.Parameters.AddWithValue("@EndDateTime", clientData.EndDateTime);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ValoresResultados valorResultado = new ValoresResultados();

                        valorResultado.IdContacto = reader[0].ToString();
                        valorResultado.IdOportunidad = reader[1].ToString();
                        valorResultado.TotalLlamadas = int.Parse(reader[2].ToString());
                        valorResultado.FechaLlamada = DateTime.Parse(reader[3].ToString());
                        valorResultado.Estatus_1 = reader[4].ToString();
                        valorResultado.Estatus_2 = reader[5].ToString();
                        valorResultado.Tipificacion = reader[6].ToString();
                        valorResultado.ValorTipificacion = int.Parse(reader[7].ToString());
                        valorResultado.VCC = reader[8].ToString();
                        valorResultado.Campana = reader[9].ToString();
                        valorResultado.scheduledDate = DateTime.Parse(reader[10].ToString());
                        valorResultado.CuandoAsiste = DateTime.Parse(reader[11].ToString());
                        valorResultado.ProgramaInteres = reader[12].ToString();
                        valorResultado.Detalle = reader[13].ToString();
                        valorResultado.QueEstudiar = reader[14].ToString();
                        valorResultado.InformacionFaltante = reader[15].ToString();

                        valoresResultados.Add(valorResultado);
                    }
                }
            }
            catch (Exception ex)
            {
                addLogErrors("getValoresResultados", ex.Message);
                //Console.WriteLine(e.ToString());
                //throw;
            }
            return valoresResultados;
        }
        // Método que permite hacer las validaciones de la Tipicación de los casos
        public static BaseDataPostMerge validacionesValoresResultados(List<ValoresResultados> valoresResultados, ClientData clientData)
        {
            List<BaseDataPost> dataPosts = new List<BaseDataPost>();
            List<BaseDataPostContact> dataPostContacts = new List<BaseDataPostContact>();
            //int contador = 0;
            foreach (var valorResultado in valoresResultados)
            {
                List<AdditionalDataPost> additionalDataPost = new List<AdditionalDataPost>();
                AdditionalDataPost adp = new AdditionalDataPost();

                if (
                    // ID-001. Estatus_1: No contactado, Estatus_2: null, Tipificaciones: Buzón de Voz, Campos adicionales: null
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_NoContacto) &&
                     string.IsNullOrEmpty(valorResultado.Estatus_2) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_BuzonDeVoz)) ||
                    // ID-002, ID-003, ID-004, ID-005.  Estatus_1: No contactado, Estatus_2:
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_NoContacto) &&
                    (valorResultado.Tipificacion.Equals(Constants.Tipificacion_ClienteContestaYCuelga) || // ID-002. Tipificaciones: Cliente contesta y cuelga 
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_NumeroSuspendidoFueraServicio) || // ID-003. Tipificaciones: Número suspendido / Fuera de servicio 
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_SeCaeLlamadaEnMarcacion) || // ID-004. Tipificaciones: Se cae la llamada en marcación 
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_TelefonoOcupado))) || // ID-005. Tipificaciones: Tipificaciones: Teléfono Ocupado 
                                                                                                     // ID-009. *Estatus 1: Contactado *Estatus_2: Reprogramar *Tipificaciones: Problemas de audio durante la llamada *Campos adicionales: null
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Reprogramar) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_ProblemasAudioEnLLamada)) ||
                    // ID–025 Tipificacion Cliente pide sacar de marcación
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Salida) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_ClientePideSacarMarcacion))
                   )
                {
                    adp = null;
                    additionalDataPost.Add(adp);
                }

                if (
                    // Tipificacion Número equivocado
                    (valorResultado.Estatus_1.Equals(Constants.Tipificacion_NumeroEquivocado)) ||
                    // ID–016, ID-018, ID-023 Tipificacion Cambio de ciclo
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Negativa) &&
                     (valorResultado.Tipificacion.Equals(Constants.Tipificacion_CambioCiclo) ||
                      valorResultado.Tipificacion.Equals(Constants.Tipificacion_LejaniaCampus) ||
                      valorResultado.Tipificacion.Equals(Constants.Tipificacion_NoSolicitoInformacion))) ||
                    // ID – 026 Tipificacion Cliente pide sacar de marcación
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Salida) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_InscritoAliat)) ||
                    // ID – 006 Tipificacion Contactado, Mensaje con terceros
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_MensajeConTerceros) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_SinContactoConInteresado)) ||
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_NoContacto) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_NumeroEquivocado))
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("Detalles", valorResultado.Detalle));
                }

                // ID-007. *Estatus 1: Contactado *Estatus: Cita *Tipificaciones: null *Campos adicionales: Fecha y hora de cita
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Tipificacion.Equals(Constants.Tipificacion_Cita))
                {
                    additionalDataPost.Add(setAdditionalDataPost("Fecha y hora de cita", valorResultado.CuandoAsiste.ToString("yyyy/MM/dd HH:mm")));
                }

                if (
                    // ID-008 *Estatus 1: Contactado *Estatus: Reprogramar *Tipificaciones: Cliente pide volver a llamar *Campos adicionales: Fecha de seguimiento
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Reprogramar) &&
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_ClientePideVolverALlamar)) ||
                    // ID–010, ID-011, ID-012 *Estatus 1: Contactado* Estatus_2: No define 
                    (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                     (valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_NoDefine) || valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_NoDefine2)) &&
                     (valorResultado.Tipificacion.Equals(Constants.Tipificacion_NoSeQueEstudiar) || // ID-010 *Tipificaciones: No sé qué estudiar *Campos adicionales: Fecha de seguimiento
                       valorResultado.Tipificacion.Equals(Constants.Tipificacion_NoEstoyListoVoyAPensar) || // ID-011 *Tipificaciones: No estoy listo / Lo voy a pensar *Campos adicionales: Fecha de seguimiento
                       valorResultado.Tipificacion.Equals(Constants.Tipificacion_RevisandoConPapasOfamiliares) || // ID-012 *Tipificaciones: Revisando con papás o familiares *Campos adicionales: Fecha de seguimiento
                       valorResultado.Tipificacion.Equals(Constants.Tipificacion_EvaluandoTemasEconomicos) || // ID–013 Evaluando temas economicos
                       valorResultado.Tipificacion.Equals(Constants.Tipificacion_EvaluandoOtrasUniversidadesProgramas) || // ID-014 Evaluando otras universidades Programas)))  
                       valorResultado.Tipificacion.Equals(Constants.Tipificacion_EnEsperaResultados))) // ID–015 Tipificacion En espera de resultados
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("Fecha de seguimiento", valorResultado.scheduledDate.ToString("yyyy/MM/dd HH:mm")));
                }

                // ID – 013, 014 tipificacion En espera de resultados
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_NoDefine) &&
                    (valorResultado.Tipificacion.Equals(Constants.Tipificacion_EnEsperaResultados) ||
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_EvaluandoOtrasUniversidadesProgramas)))
                {
                    additionalDataPost.Add(setAdditionalDataPost("¿Qué universidad/Programa?", valorResultado.ProgramaInteres));
                }

                // ID – 015 Tipificacion En espera de resultados
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_NoDefine) &&
                    valorResultado.Tipificacion.Equals(Constants.Tipificacion_EvaluandoTemasEconomicos)
                   )
                {
                    AdditionalDataPost adp2 = new AdditionalDataPost();
                    adp2.Campo = "¿Cuánto puede pagar?";
                    adp2.Valor = "100.00";
                    additionalDataPost.Add(setAdditionalDataPost("¿Cuánto puede pagar?", "100.00"));
                }

                // ID – 017 Tipificacion Horarios de clases no compatibles de clases
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Negativa) &&
                    valorResultado.Tipificacion.Equals(Constants.Tipificacion_HorariosClasesNoCompatibles)
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("¿Cuándo puede asistir a clases?", valorResultado.CuandoAsiste.ToString("yyyy/MM/dd HH:mm")));
                }

                // ID – 019, 020 Tipificacion Horarios de clases no compatibles de clases
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Negativa) &&
                    (valorResultado.Tipificacion.Equals(Constants.Tipificacion_NoExisteProgramaEnCampus) ||
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_NoExisteOfertaAcademicaAliat))
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("¿Qué quiere estudiar?", valorResultado.QueEstudiar));
                }

                // ID – 021, 022 Tipificacion Inscrito en escuela privada
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Negativa) &&
                    (valorResultado.Tipificacion.Equals(Constants.Tipificacion_InscritoEscuelaPrivada) ||
                     valorResultado.Tipificacion.Equals(Constants.Tipificacion_InscritoEscuelaPublica))
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("Universidad/Programa seleccionado", valorResultado.ProgramaInteres));
                }

                // ID – 024 Tipificacion Temas económicos
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Negativa) &&
                    valorResultado.Tipificacion.Equals(Constants.Tipificacion_TemasEconomicos)
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("¿Cuánto está dispuesto a pagar?", "10"));
                }

                // ID – 027 Tipificacion No cubre perfil / edad / grado
                if (valorResultado.Estatus_1.Equals(Constants.TipoStatus_1_Contacto) &&
                    valorResultado.Estatus_2.Equals(Constants.TipoStatus_2_Salida) &&
                    valorResultado.Tipificacion.Equals(Constants.Tipificacion_NoCubrePerfilEdadGrado)
                   )
                {
                    additionalDataPost.Add(setAdditionalDataPost("¿Qué le falta?", valorResultado.InformacionFaltante));
                }

                if (additionalDataPost.Count > 0)
                {
                    dataPosts.Add(setBaseDataPost(valorResultado, additionalDataPost));
                    dataPostContacts.Add(setBaseDataPostContact(valorResultado));
                }
                else
                {
                    // Registra aquellas tipificaciones que no aplican para los casos de uso
                    addLogNoApply(valorResultado, DateTime.Parse(clientData.StartDateTime));
                }
            }

            BaseDataPostMerge dataPostMerge = new BaseDataPostMerge();
            dataPostMerge.baseDataPosts = dataPosts;
            dataPostMerge.baseDataPostContacts = dataPostContacts;

            return dataPostMerge;
        }
        // Método para hacer Validaciones adicionales en Datapost
        public static BaseDataPost additionalValidationsBaseDataPost(BaseDataPost dataPost)
        {

            //Organizo valor default ws enviar en json

            if ((dataPost.Estatus1.Equals(Constants.TipoStatus_1_Contacto) &&
                dataPost.Estatus2.Equals(Constants.TipoStatus_2_Negativa) &&
                dataPost.Tipificacion.Equals(Constants.Tipificacion_HorariosClasesNoCompatibles)))
            {
                dataPost.Tipificacion = "Horarios de clases no compatibles";
            }
            if ((dataPost.Estatus1.Equals(Constants.TipoStatus_1_Contacto) &&
                dataPost.Estatus2.Equals(Constants.TipoStatus_2_Negativa) &&
                dataPost.Tipificacion.Equals(Constants.Tipificacion_NoExisteProgramaEnCampus)))
            {
                dataPost.Tipificacion = "No existe programa en el Campus";
            }
            if ((dataPost.Estatus1.Equals(Constants.TipoStatus_1_Contacto) &&
                 dataPost.Estatus2.Equals(Constants.TipoStatus_2_MensajeConTerceros) &&
                 dataPost.Tipificacion.Equals(Constants.Tipificacion_SinContactoConInteresado)))
            {
                dataPost.Estatus1 = "No contactado";
                dataPost.Estatus2 = null;
                dataPost.Tipificacion = "Número equivocado";
            }
            if (dataPost.Estatus1.Equals(Constants.TipoStatus_1_NoContacto))
            {
                dataPost.Estatus1 = "No contactado";
            }
            if (dataPost.Estatus1.Equals(Constants.TipoStatus_1_Contacto))
            {
                dataPost.Estatus1 = "Contactado";
            }
            if (!string.IsNullOrEmpty(dataPost.Estatus2) && dataPost.Estatus2.Equals(Constants.TipoStatus_2_NoDefine))
            {
                dataPost.Estatus2 = "No define";
            }
            return dataPost;
        }
        // Método para llenar AdditionalDataPost según la regla de tipificación que se tenga
        public static AdditionalDataPost setAdditionalDataPost(string campo, string valor)
        {
            AdditionalDataPost additionalDataPost = new AdditionalDataPost();
            additionalDataPost.Campo = campo;
            additionalDataPost.Valor = valor;
            return additionalDataPost;
        }
        // Metodo para llenar BaseDataPost con los datos de valores, este objeto es el que se envia para que sea en el Servicio
        public static BaseDataPost setBaseDataPost(ValoresResultados br, List<AdditionalDataPost> additionalDataPosts)
        {
            BaseDataPost bdp = new BaseDataPost();

            if (additionalDataPosts[0] == null) additionalDataPosts = null;

            bdp.IdOportunidad = br.IdOportunidad;
            bdp.TotalLlamadas = br.TotalLlamadas.ToString();
            bdp.FechaLlamada = br.FechaLlamada.ToString("yyyy/MM/dd HH:mm");
            bdp.Estatus1 = br.Estatus_1;
            bdp.Estatus2 = br.Estatus_2;
            bdp.Tipificacion = br.Tipificacion;
            bdp.CamposAdicionales = additionalDataPosts;
            //bdp.CamposAdicionales = (additionalDataPosts[0]==null?null:additionalDataPosts);

            return bdp;
        }
        // Metodo para llenar BaseDataPost con los datos de valores, este objeto es el que se envia para que sea en el Servicio
        public static BaseDataPostContact setBaseDataPostContact(ValoresResultados br)
        {
            BaseDataPostContact bdp = new BaseDataPostContact();

            bdp.ContactId = br.IdContacto;
            bdp.IdOportunidad = br.IdOportunidad;

            return bdp;
        }
        private static void addLogNoApply(ValoresResultados valorResultado, DateTime processDate)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spLogNoApply", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProcessDate", processDate);
                    cmd.Parameters.AddWithValue("@IdOportunidad", valorResultado.IdOportunidad);
                    cmd.Parameters.AddWithValue("@Contactid", valorResultado.IdContacto);
                    cmd.Parameters.AddWithValue("@Campaign", valorResultado.Campana);
                    cmd.Parameters.AddWithValue("@Estatus1", valorResultado.Estatus_1);
                    cmd.Parameters.AddWithValue("@Estatus2", valorResultado.Estatus_2);
                    cmd.Parameters.AddWithValue("@Tipificacion", valorResultado.Tipificacion);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error al incluir registro en LogWsTipificacion: ", e);
                addLogErrors("addLogNoApply", ex.Message, valorResultado.Campana, valorResultado.IdContacto, valorResultado.IdOportunidad);
            }
        }
        // Método que permite incluir los registros de los LOGS por cada tipificación
        private static void addLogWsTipificacion(BaseDataPost baseDataPost, ResponseService rs, BaseDataPostContact bdpc, string cd, ValoresResultados valoresResultados)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spLogWSTipification", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaEnvioWS", DateTime.Now);
                    cmd.Parameters.AddWithValue("@TipoPost", "POST");
                    cmd.Parameters.AddWithValue("@IdOportunidad", baseDataPost.IdOportunidad);
                    cmd.Parameters.AddWithValue("@FechaLlamada", Convert.ToDateTime(baseDataPost.FechaLlamada).ToString("yyyy-MM-dd HH:mm"));
                    cmd.Parameters.AddWithValue("@Llamadas", baseDataPost.TotalLlamadas);
                    cmd.Parameters.AddWithValue("@Estatus_1", baseDataPost.Estatus1);
                    cmd.Parameters.AddWithValue("@Estatus_2", baseDataPost.Estatus2);
                    cmd.Parameters.AddWithValue("@Tipificacion", baseDataPost.Tipificacion);
                    cmd.Parameters.AddWithValue("@FechaCita", valoresResultados.CuandoAsiste);
                    cmd.Parameters.AddWithValue("@FechaSeguimiento", valoresResultados.scheduledDate);
                    cmd.Parameters.AddWithValue("@Universidad", null);
                    cmd.Parameters.AddWithValue("@Pagar", null);
                    cmd.Parameters.AddWithValue("@Detalles", valoresResultados.Detalle);
                    cmd.Parameters.AddWithValue("@QuiereEstudiar", valoresResultados.QueEstudiar);
                    cmd.Parameters.AddWithValue("@Falta", valoresResultados.InformacionFaltante);
                    cmd.Parameters.AddWithValue("@RequestWS", JsonConvert.SerializeObject(baseDataPost));
                    cmd.Parameters.AddWithValue("@ResponseWS", JsonConvert.SerializeObject(rs));
                    cmd.Parameters.AddWithValue("@Campaña", cd);
                    cmd.Parameters.AddWithValue("@Contactid", bdpc.ContactId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error al incluir registro en LogWsTipificacion: ", e);
                addLogErrors("addLogWsTipificacion", ex.Message, cd, bdpc.ContactId, baseDataPost.IdOportunidad);
            }
        }
        // Método que permite incluir los registros de los contactos por fecha
        private static void addControlEnvioWS(string contactId, string dateProcess)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spTbControlEnvioWS", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContactId", contactId);
                    cmd.Parameters.AddWithValue("@TmStmp", DateTime.Parse(dateProcess));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                //Console.WriteLine("Error al incluir registro en ControlEnvioWS: ", e);
                addLogErrors("addControlEnvioWS", ex.Message, null, contactId);
            }
        }
        private static void updateProcessDateTipification(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spProcessDateTipificacionUpdate", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error al incluir registro en LogWsTipificacion: ", e);
                addLogErrors("updateProcessDateTification", ex.Message);
            }
        }
        // Método que permite listar los parámetros de configuración de la API del cliente
        public static List<EntityValue> getApiSettings()
        {
            return getEntityValues(Constants.ParametrosServicios);
        }
        // Método para consumir el servicio del Cliente
        public static async Task apiPost(List<BaseDataPost> dataPosts, List<BaseDataPostContact> dataPostContacts, ClientData clientData, List<ValoresResultados> valoresResultados)
        {
            List<EntityValue> settingsApi = getApiSettings();

            List<MailConfig> mailConfigList = MailProcess.getMailConfigList();

            try
            {
                HttpClient httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(settingsApi[0].Name);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                using (httpClient)
                {
                    try
                    {
                        foreach (var iDataPost in dataPosts)
                        {
                            ResponseService responseService = new ResponseService();

                            if (!string.IsNullOrEmpty(iDataPost.FechaLlamada.ToString()))
                            {
                                string dtt = Convert.ToDateTime(iDataPost.FechaLlamada).ToString("yyyy/MM/dd HH:mm");

                                iDataPost.FechaLlamada = dtt;
                            }

                            List<BaseDataPost> dataPost = new List<BaseDataPost>();

                            dataPost.Add(additionalValidationsBaseDataPost(iDataPost));

                            //string json = '['+JsonConvert.SerializeObject(dataPost)+']';
                            string json = JsonConvert.SerializeObject(dataPost);

                            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                            int retryCounter = 0;

                            while (retryCounter < Constants.RetriesSendingData)
                            {
                                retryCounter++;

                                HttpResponseMessage response = await httpClient.PostAsync(settingsApi[1].Name, httpContent);

                                string responseBody = await response.Content.ReadAsStringAsync();

                                //Console.WriteLine("IdOportunidad: " + iDataPost.IdOportunidad);

                                //if (iDataPost.IdOportunidad.Equals("Oportunidad-3260525"))
                                //{
                                //    Console.WriteLine("IdOportunidad: " + iDataPost.IdOportunidad);
                                //}

                                // Deserializa la respuesta del servicio
                                responseService = JsonConvert.DeserializeObject<ResponseService>(responseBody);
                                BaseDataPostContact dpc = dataPostContacts.Find(m => m.IdOportunidad == dataPost[0].IdOportunidad);
                                ValoresResultados vr = valoresResultados.Find(m => m.IdOportunidad == dataPost[0].IdOportunidad);

                                

                                string message = "";

                                if (responseService.Mensaje.Equals("No hay Oportunidades a Procesar.")) retryCounter = 3;

                                if (response.IsSuccessStatusCode)
                                {
                                    if (!string.IsNullOrEmpty(responseService.Detalles.ToString()))
                                    {
                                        message = "Mensaje: " + responseService.Mensaje + " ID: " + dataPost[0].IdOportunidad + " Detalle: " + responseService.Detalles[0].Mensaje;
                                    }
                                    else
                                    {
                                        message = "Mensaje: " + responseService.Mensaje + " ID: " + dataPost[0].IdOportunidad + " Detalle: " + responseService.Detalles;
                                    }

                                    Log(message, clientData, mailConfigList[0].LogFileName);
                                    // Adiciona los registros de LOG y Control de Envio WS si es satisfactoria la actualización

                                    addLogWsTipificacion(dataPost[0], responseService, dpc, clientData.Campaign, vr);
                                    addControlEnvioWS(getContactId(dataPost[0].IdOportunidad, dataPostContacts), clientData.StartDateTime.Substring(0, 10));
                                    retryCounter = 3;
                                }
                                else
                                {
                                    if (responseService.Detalles == null)
                                    {
                                        message = "Mensaje: " + (responseService.Mensaje != null ? responseService.Mensaje : "No tiene mensaje") +
                                                  " ID: " + dataPost[0].IdOportunidad + " Detalle: No tiene detalle";
                                        responseService.Detalles = new List<ResponseServiceDetail>();
                                    }
                                    else
                                    {
                                        message = "Mensaje: " + responseService.Mensaje + " ID: " + dataPost[0].IdOportunidad + " Detalle: " + responseService.Detalles[0].Mensaje;
                                    }

                                    addLogWsTipificacion(dataPost[0], responseService, dpc, clientData.Campaign, vr);
                                    Log(message, clientData, mailConfigList[1].LogFileName);
                                }
                            }
                        }
                        updateProcessDateTipification(clientData.Id);
                        // Envia correos
                        sendMailWithFile(clientData, mailConfigList);
                    }
                    catch (InvalidOperationException ex)
                    {
                        addLogErrors("apiPost", ex.Message, clientData.Campaign);
                    }
                    catch (HttpRequestException ex)
                    {
                        addLogErrors("apiPost", ex.Message, clientData.Campaign);
                    }
                    catch (TaskCanceledException ex)
                    {
                        addLogErrors("apiPost", ex.Message, clientData.Campaign);
                    }
                    catch (UriFormatException ex)
                    {
                        addLogErrors("apiPost", ex.Message, clientData.Campaign);
                    }
                }
            }
            catch (Exception ex)
            {
                addLogErrors("apiPost", ex.Message);
            }
        }
        public static void addLogErrors(string method, string errorDescription, string campaign = null, string contactId = null, string idOportunidad = null)
        {
            DateTime hoy = DateTime.Now.Date;
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spLogError", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@dateLog", hoy);
                    cmd.Parameters.AddWithValue("@method", method);
                    cmd.Parameters.AddWithValue("@errorDescription", errorDescription);
                    cmd.Parameters.AddWithValue("@campaign", campaign);
                    cmd.Parameters.AddWithValue("@contactid", contactId);
                    cmd.Parameters.AddWithValue("@idoportunidad", idOportunidad);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Error al incluir registro en ControlEnvioWS: ", e);
                //addLogErrors("apiPost", ex.Message);
            }
        }
        // Metodo para buscar en la colección el contactid por Id de Oportunidad
        private static string getContactId(string idOportunidad, List<BaseDataPostContact> dataPostContacts)
        {
            foreach (var contact in dataPostContacts)
            {
                if (contact.IdOportunidad.Equals(idOportunidad)) return contact.ContactId;
            }
            return "";
        }
        // Método para crear el archivo LOG Satisfactorio y No Satisfactorio
        private static void Log(string mensaje, ClientData clientData, string fileName)
        {
            string filename = makeTextFileName(fileName, clientData.Campaign, clientData.StartDateTime);

            using (StreamWriter w = File.AppendText(filename))
            {
                Log(mensaje, w);
            }
        }
        // Método para guardar un LOG por cada registro que se envia al Servicio del Cliente
        private static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine("\r\nRegistro de Log: {0} {1}: {2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), logMessage);
        }
        // Método para sustituir las variables que están en el Subject del mensaje, o para el nombre del archivo LOG
        public static string makeTextFileName(string fileName, string campaign, string date)
        {
            string filename = fileName.Replace("&campaign", campaign).Replace("&processDate", date.Substring(0, 10)).Replace("/", "-");
            return filename;
        }
        // Método para enviar los archivos LOG por correo a los destinatarios
        public static void sendMailWithFile(ClientData clientData, List<MailConfig> mailConfigs)
        {
            string[] filename = new string[1];
            //string[] filename = new string[mailConfigs.Count];
            int i = 0;
            foreach (var mailConfig in mailConfigs)
            {
                filename[i] = makeTextFileName(mailConfig.LogFileName, clientData.Campaign, clientData.StartDateTime);
                if (File.Exists(filename[i])) MailProcess.sendMailMessage(mailConfig, filename, clientData.Campaign, clientData.StartDateTime);
            }
        }
        // Método para enviar los archivos LOG por correo a los destinatarios
        public static void sendMailWithManyFiles(ClientData clientData, List<MailConfig> mailConfigs)
        {
            foreach (var mailConfig in mailConfigs)
            {
                string filename = makeTextFileName(mailConfig.LogFileName, clientData.Campaign, clientData.StartDateTime);
                //if (File.Exists(filename)) MailProcess.sendMailMessage(mailConfig, filename, clientData.Campaign, clientData.StartDateTime);
            }
        }
    }
}