using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Sockets;
using DataCalculationsLib.DataModelsDBML;
using DataCalculationsLib.DataModelsEntity;
using DataCalculationsLib.ExceptionHadler;

namespace DataCalculationsLib
{
    public class MillPlusFormatControlDb
    {
        public bool GetListOfAvailableMFunc(out List<int> availableMfuncList, out string errMsg)
        {
            errMsg = "";
            availableMfuncList=new List<int>();
            using (var dc = ContextCreator.GetEntityContext())
            {
                var q = dc.E_MILLPLUS_AVAILABLE_M_FUNCTIONS
                    .Select(s=>s.M_FUNC);
                try
                {
                    availableMfuncList = q.ToList();
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
                if (availableMfuncList.Count == 0)
                {
                    errMsg = "Не найден список поддерживаемых функций";
                    return false;
                }
                return true;
            }
        }

        public bool GetListOfAvailableGFunc(out List<int> availableGfuncList, out string errMsg)
        {
            errMsg = "";
            availableGfuncList=new List<int>();
            using (var dc = ContextCreator.GetEntityContext())
            {
                var q = dc.E_MILLPLUS_AVAILABLE_G_FUNCTIONS
                    .Select(s => s.G_FUNC);
                try
                {
                    availableGfuncList = q.ToList();
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
                if (availableGfuncList.Count == 0)
                {
                    errMsg = "Не найден список поддерживаемых функций";
                    return false;
                }
                return true;
            }
        }
    }
}
