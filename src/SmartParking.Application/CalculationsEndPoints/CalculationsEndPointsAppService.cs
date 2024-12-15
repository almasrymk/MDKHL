using Microsoft.AspNetCore.Mvc;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SmartParking.CalculationsEndPoints;
public class CalculationsEndPointsAppService : ITransientDependency, ICalculationsEndPointsAppService 
{

    public int CalculateTime(DateTime startTime, DateTime endTime, int FreeMinutes)
    {


        // Calculate the time difference
        TimeSpan timeDifference = endTime - startTime;



        if ((int)timeDifference.TotalHours == 0 && timeDifference.Minutes <= FreeMinutes)
        {
            return 0;

        }

        int hoursDiff = (int)timeDifference.TotalHours;


        hoursDiff += (timeDifference.Minutes > 0 ? 1 : 0);


        return  hoursDiff;
    }

    public  decimal CalculatePriceForExceededHours(int totalHours, Organization organization)
    {
        decimal price = 0;
        if (totalHours > organization.NumberOfHours)
        {
            int fullDaysCustomerHasStayed = 0;
            if (totalHours >= 24)
            {
                fullDaysCustomerHasStayed = totalHours / 24;
                totalHours %= 24;
            }
            else
            {
                fullDaysCustomerHasStayed = 1;
                totalHours = 0;
            }
            price += (fullDaysCustomerHasStayed * organization.FullDayPrice);
        }
        price += CalculatePriceHours(organization, totalHours);
        return price;

    }
    public  decimal CalculatePriceForOrganizationIsClosedAndExceededHours(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime)
    {
        (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
        var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
        double price = 0;
        double totalDays = Math.Ceiling(specialHours / closingHour);
        price += totalDays * (double)organization.FullDayPrice;
        price += ((int)(normalHours / 24)) * (double)organization.FullDayPrice;
        normalHours -= ((int)(normalHours / 24)) * 24;
        if (normalHours > organization.NumberOfHours)
        {
            price += (double)organization.FullDayPrice;
            normalHours = 0;
        }
        price += (double) CalculatePriceHours(organization, (int)normalHours);
        return (decimal)price;

    }
    public  decimal CalculatePriceForOrganizationIsClosed(int totalHours, Organization organization, CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime)
    {

        //special hours -> hours where organization is close
        //normal hours -> hours where organization is open 
        (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
        var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
        double price = 0;
        double totalDays = Math.Ceiling(specialHours / closingHour);
        // calculates full days price
        price += totalDays * (double)organization.FullDayPrice;
        //calculates hours
        if ((int)normalHours + (int)specialHours > 24)
        {
            price += (double)  CalculatePriceHoursExceededDay(organization, (int)normalHours + (int)specialHours);
            price -= totalDays * closingHour * (double)organization.AdditionalHourPrice;

        }
        else
        {
            price += (double)  CalculatePriceHoursExceededDay(organization, (int)normalHours);

        }
        return (decimal)price;
    }
    public  decimal CalculatePriceHours(Organization organization, int hours)
    {
        decimal price = 0;
        price += hours > 0 ? organization.FirstHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.SecondHourPrice : 0;
        hours--;
        price += hours > 0 ? organization.ThirdHourPrice : 0;
        hours--;
        price += hours > 0 ? (hours * organization.AdditionalHourPrice) : 0;
        return price;
    }
    public decimal CalculatePriceHoursExceededDay(Organization organization, int hours)
    {
        decimal price = 0;
        while (hours > 0)
        {
            price += hours > 0 ? organization.FirstHourPrice : 0;
            hours--;
            price += hours > 0 ? organization.SecondHourPrice : 0;
            hours--;
            price += hours > 0 ? organization.ThirdHourPrice : 0;
            hours--;
            price += hours > 0 ? (Math.Min(hours, 21) * organization.AdditionalHourPrice) : 0;
            hours -= 21;

        }
        return price;
    }
    public (double, double) CalculateHours(CreateUpdateGarageTransactionDto garageTransaction, DateTime entrenceTime, Organization organization)
    {
        TimeSpan totalDuration = garageTransaction.AccurateTime.Value - entrenceTime;

        double totalHours = (int)totalDuration.TotalHours;
        totalHours += totalDuration.Minutes > 0 ? 1 : 0;


        double specialHours = 0;

        // Loop through each day from entry date to exit date
        DateTime currentDate = entrenceTime.Date;
        while (currentDate <= garageTransaction.AccurateTime.Value.Date)
        {
            DateTime specialStartTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursStart.Hour, organization.ClosingHoursStart.Minute, 0);
            DateTime specialEndTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, organization.ClosingHoursEnd.Hour, organization.ClosingHoursEnd.Minute, 0);

            // Handle overnight case where special hours span two dates
            if (specialEndTime.Hour < specialStartTime.Hour)
            {
                specialEndTime = specialEndTime.AddDays(1);

                // Calculate hours before midnight
                if (entrenceTime < specialEndTime && garageTransaction.AccurateTime >= specialStartTime)
                {
                    DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDurationBeforeMidnight = effectiveEnd - effectiveStart;
                    if (specialDurationBeforeMidnight.TotalHours > 0)
                    {
                        specialHours += (int)specialDurationBeforeMidnight.TotalHours; // Round up
                        specialHours += specialDurationBeforeMidnight.Minutes > 0 ? 1 : 0;
                    }
                }

                // Calculate hours after midnight
                if (garageTransaction.AccurateTime > specialEndTime && garageTransaction.AccurateTime >= specialEndTime)
                {
                    DateTime effectiveStart = specialEndTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDurationAfterMidnight = effectiveEnd - effectiveStart;
                    if (specialDurationAfterMidnight.TotalHours > 0)
                    {
                        specialHours += (int)specialDurationAfterMidnight.TotalHours; // Round up
                        specialHours += specialDurationAfterMidnight.Minutes > 0 ? 1 : 0;
                    }
                }

            }
            else
            {
                // Normal case where special hours are within the same day
                if (entrenceTime < specialEndTime && garageTransaction.AccurateTime > specialStartTime)
                {
                    DateTime effectiveStart = entrenceTime < specialStartTime ? specialStartTime : entrenceTime;
                    DateTime effectiveEnd = garageTransaction.AccurateTime.Value < specialEndTime ? garageTransaction.AccurateTime.Value : specialEndTime;

                    TimeSpan specialDuration = effectiveEnd - effectiveStart;
                    if (specialDuration.TotalHours > 0)
                    {
                        specialHours += (int)specialDuration.TotalHours; // Round up
                        specialHours += specialDuration.Minutes > 0 ? 1 : 0;
                    }
                }
            }

            // Move to the next day
            currentDate = currentDate.AddDays(1);
        }

        // Normal hours are total hours minus special hours
        double normalHours = totalHours - specialHours;
        return (specialHours, normalHours);
    }

    public int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto, DateTime? PackageExpiryDate)
    {
        if (exitTime < entranceTime)
        {
            throw new BusinessException("Leaving date can't be befor entrance date");
        }
        if (packageDto.Package.Published)
        {
            return 0;
        }
        DateTime packageStart = entranceTime.Value.Date.AddHours(packageDto.Package.PublishFrom.Hour).AddMinutes(packageDto.Package.PublishFrom.Minute);
        DateTime packageEnd = entranceTime.Value.Date.AddHours(packageDto.Package.PublishTo.Hour).AddMinutes(packageDto.Package.PublishTo.Minute);

        double extraHours = 0;

        DateTime currentDay = entranceTime.Value;
        DateTime endDay = exitTime.Value;

        while (currentDay <= Min(PackageExpiryDate.Value, exitTime.Value))
        {
            DateTime dayPackageStart = currentDay.Date + packageStart.TimeOfDay;
            DateTime dayPackageEnd = currentDay.Date + packageEnd.TimeOfDay;

            if (packageEnd < packageStart)
            {
                dayPackageEnd = dayPackageEnd.AddDays(1);
            }

            DateTime effectiveStart = Max(currentDay, dayPackageStart);
            DateTime effectiveEnd = Min(endDay, dayPackageEnd);



            if (currentDay < dayPackageStart)
            {
                var smallestDay = (Min(endDay, dayPackageStart) - currentDay);
                extraHours += smallestDay.Hours + (smallestDay.Minutes > 0 ? 1 : 0);
                currentDay = dayPackageStart;

            }
            if (currentDay.TimeOfDay > dayPackageEnd.TimeOfDay)
            {
                var hours = currentDay.Hour - dayPackageEnd.Hour;
                currentDay.AddHours(hours);
                extraHours -= hours;
            }

            if (endDay > dayPackageEnd && endDay.Date == currentDay.Date)
            {
                extraHours += (endDay - Max(currentDay, dayPackageEnd)).TotalHours;
                extraHours += (endDay - Max(currentDay, dayPackageEnd)).Minutes > 0 ? 1 : 0;
            }

            if (endDay.Date > currentDay.Date)
            {
                extraHours += 24 - Math.Abs(dayPackageStart.Hour - dayPackageEnd.Hour) + (Math.Abs(dayPackageStart.Minute - dayPackageEnd.Minute) > 0 ? 1 : 0);
            }
            currentDay = currentDay.AddDays(1);
            if (currentDay.Date == exitTime.Value.Date)
            {
                if (exitTime.Value.Hour < packageStart.Hour)
                {
                    extraHours -= packageStart.Hour - exitTime.Value.Hour;
                }
            }
        }


        if (endDay > PackageExpiryDate)
        {
            extraHours -= packageStart.Hour;
            extraHours += (endDay - PackageExpiryDate).Value.Hours;
            extraHours += (endDay - currentDay).Minutes > 0 ? 1 : 0;
        }

        // Return result
        return  (int)extraHours;
    }
    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

  
}
