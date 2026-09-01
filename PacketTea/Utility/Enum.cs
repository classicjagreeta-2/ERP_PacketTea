using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Utility
{
    public class Enum
    {
    }
    public enum UserPermissions
    {
        ViewCars,
        EditCars,
        DeleteCars,
        ViewUsers,
        EditUsers,
        DeleteUsers
    }

    public enum PaymentTypeEnum
    {
        Payment = 1,
        Receipt = 2,
        Others,
        ViewUsers,
        EditUsers,
        DeleteUsers
    }
    
}