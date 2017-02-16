using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using DataCalculationsLib.DataModelsDBML;

namespace DataCalculationsLib
{
    class MachineDB
    {
        /// <summary>
        /// Возвращает ключевые характеристики первого найденного станка.
        /// </summary>
        /// <param name="machineId"></param>
        /// <param name="bMin">минимальный угол по оси В</param>
        /// <param name="bMax">максимальный угол по оси В</param>
        /// <param name="spinSpeedMax">ьфксимальная скорость вращения шпинделя</param>
        /// <param name="errMsg"></param>
        /// <returns>Еси ничего не найдено, возвращает углы -360+360 и скорость 1234567890 </returns>
        public bool GetMachineParams(int machineId, out decimal bMin, out decimal bMax, out int spinSpeedMax, out string errMsg)
        {
            using (var dc = ContextCreator.GetEntityContext())
            {
                bMin = -360;
                bMax = 360;
                spinSpeedMax = 1234567890;
                errMsg = "";
                try
                {
                    var query = dc.E_MACHINE_SPISOK
                        .Where(w => w.Id_marka_machine == machineId)
                        .Select(s => s).First();
                    if (query.Fourth_axis_min != null)
                    {
                        bMin = (decimal)query.Fourth_axis_min;
                    }
                    if (query.Fourth_axis_max != null)
                    {
                        bMax = (decimal)query.Fourth_axis_max;
                    }
                    if (query.SpinMaxSpeed != null)
                    {
                        spinSpeedMax = (int)query.SpinMaxSpeed;
                    }

                }
                catch (EntityException)
                {
                    errMsg = Resources.DBConnectionProblem;
                    return false;
                }
                catch (InvalidOperationException exp)
                {
                    errMsg = exp.Message;
                    return false;
                }

            }
            return true;
        }
    }
}
