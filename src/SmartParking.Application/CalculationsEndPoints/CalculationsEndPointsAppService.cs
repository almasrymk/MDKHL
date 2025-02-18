using Microsoft.AspNetCore.Mvc;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System;
using SmartParking.Enums;
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

        (double specialHours, double normalHours) = CalculateHours(garageTransaction, entrenceTime, organization);
        var closingHour = Math.Ceiling((organization.ClosingHoursEnd - organization.ClosingHoursStart).TotalHours);
        double price = 0;
        double totalDays = Math.Ceiling(specialHours / closingHour==0?1: closingHour);
        price += totalDays * (double)organization.FullDayPrice;
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

    public int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto)
    {
        TimeValidation(entranceTime, exitTime);
        entranceTime=RemoveSecondsFromTime(entranceTime.Value);
        exitTime = RemoveSecondsFromTime(exitTime.Value);
        packageDto.ExpiryDate = RemoveSecondsFromTime(packageDto.ExpiryDate);

        if (packageDto.publishedOfPackage.Value)
        {
            return 0;
        }
        DateTime packageStart = entranceTime.Value.Date
         .AddHours(packageDto.PublishFromOfPackage.Value.Hour)
         .AddMinutes(packageDto.PublishFromOfPackage.Value.Minute);

        DateTime packageEnd = entranceTime.Value.Date
           .AddHours(packageDto.PublishToOfPackage.Value.Hour)
           .AddMinutes(packageDto.PublishToOfPackage.Value.Minute);

        if (packageEnd < packageStart)
        {
            packageEnd = packageEnd.AddDays(1);
        }
        DateTime minimumDate = MinDate(exitTime.Value, packageDto.ExpiryDate);
        int _totalHoursOfStaying = (int)Math.Ceiling((minimumDate - entranceTime.Value).TotalHours);
        int _daysOfStaying = _totalHoursOfStaying / 24;
        _totalHoursOfStaying %= 24;
        int _hoursOfPackage = (int)Math.Ceiling((MinDate(packageEnd, packageDto.ExpiryDate) - packageStart).TotalHours);
        int _hoursOutOfPackage = (24 - _hoursOfPackage) * _daysOfStaying;
        if (entranceTime.Value < packageStart)
        {
            if (minimumDate <= packageStart)
            {
                _hoursOutOfPackage += (_totalHoursOfStaying > 0 ? CalculatesHoursBetweenTwoTimes(minimumDate, entranceTime.Value) + HoursOfStayingOutsidePackageExpiration(packageDto.ExpiryDate, exitTime.Value) : 0);
            }
            else if (minimumDate > packageStart && minimumDate < packageEnd)
            {
                _hoursOutOfPackage += (_totalHoursOfStaying > 0 ? CalculatesHoursBetweenTwoTimes(packageStart, entranceTime.Value) + HoursOfStayingOutsidePackageExpiration(packageDto.ExpiryDate, exitTime.Value) : 0);

            }
            else
            {
                _hoursOutOfPackage += (_totalHoursOfStaying > 0 ? CalculatesHoursBetweenTwoTimes(packageStart, entranceTime.Value) + CalculatesHoursBetweenTwoTimes(packageEnd, minimumDate) + HoursOfStayingOutsidePackageExpiration(packageDto.ExpiryDate, exitTime.Value) : 0);

            }
        }
        else if (minimumDate >= packageStart && entranceTime.Value <= packageEnd)
        {
            if (exitTime <= packageEnd)
            {
                return _hoursOutOfPackage;
            }
            else
            {
                _hoursOutOfPackage += (_totalHoursOfStaying > 0 ? CalculatesHoursBetweenTwoTimes(minimumDate, packageEnd) + HoursOfStayingOutsidePackageExpiration(packageDto.ExpiryDate, exitTime.Value) : 0);
            }

        }
        else if (entranceTime.Value > packageEnd)
        {
            _hoursOutOfPackage += (_totalHoursOfStaying > 0 ? CalculatesHoursBetweenTwoTimes(minimumDate, entranceTime.Value) + HoursOfStayingOutsidePackageExpiration(packageDto.ExpiryDate, exitTime.Value) : 0);
        }
        return _hoursOutOfPackage;
    }

    private static DateTime RemoveSecondsFromTime(DateTime myDate)
    {
        myDate = myDate.AddSeconds(-myDate.Second);
        myDate = myDate.AddMilliseconds(-myDate.Millisecond);
        myDate = myDate.AddMicroseconds(-myDate.Microsecond);
        return myDate;
    }

    private static void TimeValidation(DateTime? entranceTime, DateTime? exitTime)
    {
        if (exitTime < entranceTime)
        {
            throw new BusinessException("Leaving date can't be befor entrance date");
        }

    }

    private static int CalculatesHoursBetweenTwoTimes(DateTime PackageStartTime, DateTime PackageEndTime)
    {
        return (int)Math.Ceiling((decimal)Math.Abs((PackageEndTime.TimeOfDay - PackageStartTime.TimeOfDay).TotalHours));
    }
    private int HoursOfStayingOutsidePackageExpiration(DateTime ExpiryDate, DateTime ExitTime)
    {
        return ExpiryDate < ExitTime ? Math.Abs((int)Math.Ceiling((decimal)(ExpiryDate - ExitTime).Hours)) : 0;

    }
    public static DateTime MinDate(DateTime T1, DateTime T2) => (T1.Date.Date <= T2.Date.Date && T1.Date.TimeOfDay <= T2.Date.TimeOfDay) ? T1 : T2;
 



}
