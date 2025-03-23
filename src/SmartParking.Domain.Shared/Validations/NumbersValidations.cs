using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations;

public static class NumbersValidations
{


    public static string validateNumber0to9999(int value)
    {
        if (value < 0 || value > 9999)
            return LocalizationHelper.GetLocalizedString("InvalidValidation");
        return null;
    }
    public static string ValidateNumber11111to99999(int value)
    {
        if (value < 11111 || value > 99999)
            return LocalizationHelper.GetLocalizedString("InvalidValidation");
        return null;
    }
      public static string ValidateNumber1111to9999(int value)
    {
        if (value < 1111 || value > 9999)
            return LocalizationHelper.GetLocalizedString("InvalidValidation");
        return null;
    }


    public static void DistributeRemainingCapacity(ref int MaxSlotsForEmployess ,ref int MaxSlotsForCustomers, ref int MaxSlotsForFree , int MaxCapacity)
    {
        int totalAllocated = MaxSlotsForEmployess
                           + MaxSlotsForCustomers
                           + MaxSlotsForFree;

        int remainingCapacity = MaxCapacity - totalAllocated;

        if (remainingCapacity < 0)
        {
            MaxSlotsForEmployess = 0;
            MaxSlotsForCustomers = 0;
            MaxSlotsForFree = 0;
        }
        else
        {
            if (MaxSlotsForEmployess > 0)
            {
                if (MaxSlotsForCustomers > 0)
                {
                    if (MaxSlotsForFree > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            // If total slots exceed MaxCapacity, reset to prevent overflow
                            ResetToZeros(out MaxSlotsForEmployess, out MaxSlotsForCustomers, out MaxSlotsForFree);
                        }
                    }
                    else
                    {
                        MaxSlotsForFree = MaxCapacity - MaxSlotsForEmployess - MaxSlotsForCustomers;

                    }

                }
                else if (MaxSlotsForFree > 0)
                {
                    if (MaxSlotsForCustomers > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            ResetToZeros(out MaxSlotsForEmployess, out MaxSlotsForCustomers, out MaxSlotsForFree);

                        }

                    }
                    else
                    {

                        MaxSlotsForCustomers = MaxCapacity - MaxSlotsForEmployess - MaxSlotsForFree;
                    }

                }
            }
            else if (MaxSlotsForCustomers > 0)
            {
                if (MaxSlotsForFree > 0)
                {
                    if (MaxSlotsForEmployess > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            ResetToZeros(out MaxSlotsForEmployess, out MaxSlotsForCustomers, out MaxSlotsForFree);

                        }

                    }
                    else
                    {

                        MaxSlotsForEmployess = MaxCapacity - MaxSlotsForCustomers - MaxSlotsForFree;
                    }
                }
                else if (MaxSlotsForEmployess > 0)
                {
                    if (MaxSlotsForFree > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            ResetToZeros(out MaxSlotsForEmployess, out MaxSlotsForCustomers, out MaxSlotsForFree);

                        }

                    }
                    else
                    {
                        MaxSlotsForFree = MaxCapacity - MaxSlotsForEmployess - MaxSlotsForCustomers;

                    }

                }
            }
            else if (MaxSlotsForFree > 0)
            {
                if (MaxSlotsForEmployess > 0)
                {
                    if (MaxSlotsForCustomers > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            ResetToZeros(out MaxSlotsForEmployess, out MaxSlotsForCustomers, out MaxSlotsForFree);

                        }

                    }
                    else
                    {
                            MaxSlotsForCustomers = MaxCapacity - MaxSlotsForEmployess - MaxSlotsForFree;

                    }
                }
                else if (MaxSlotsForCustomers > 0)
                {
                    if (MaxSlotsForEmployess > 0)
                    {
                        if (remainingCapacity < 0)
                        {
                            ResetToZeros(out MaxSlotsForEmployess, out MaxSlotsForCustomers, out MaxSlotsForFree);

                        }

                    }
                    else
                    {

                        MaxSlotsForEmployess = MaxCapacity - MaxSlotsForFree - MaxSlotsForCustomers;
                    }

                }
            }
        }
    }

    private static void ResetToZeros(out int MaxSlotsForEmployess, out int MaxSlotsForCustomers, out int MaxSlotsForFree)
    {
        MaxSlotsForEmployess = 0;
        MaxSlotsForCustomers = 0;
        MaxSlotsForFree = 0;
    }

    public static string ValidateInvoice(decimal discountValue, decimal InvoicePriceWithVat)
    {
        if (discountValue < 0)
            return LocalizationHelper.GetLocalizedString("DiscountInvalidationLessThanZero");
        if (discountValue > InvoicePriceWithVat)
        {
            return LocalizationHelper.GetLocalizedString("DiscountInvalidationGreater");
        }
        
        return null;
    }

}
