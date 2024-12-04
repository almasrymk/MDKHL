using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.User;

partial class EditUserDialog
{
        [Parameter]
        public string userId { get; set; }
        [Parameter]
        public UpdateUserDto updateUserDto { get; set; }

        public async Task SubmitEdit()
        {
            var successedMessage = L["UserUpdateSuccessfullyWith"];
            try
            {


                var userUpdated = await UsersAppService.UpdateAsync(Guid.Parse(userId), updateUserDto);
                if (userUpdated != null)
                {

                    ShowSnackbar($" {successedMessage} {userUpdated.Name}", Severity.Success);
                    await Task.Delay(1000);
                    Navigation.NavigateTo("/users", true);



                }


            }
            catch (AbpValidationException abpValidationException)
            {
                string errors = string.Empty;
                foreach (var error in abpValidationException.ValidationErrors)
                {
                    errors += error;
                }
                ShowSnackbar(errors, Severity.Error);

            }
            catch (BusinessException businessException)
            {
                string errors = string.Empty;
                foreach (var error in businessException.Message)
                {
                    errors += error;
                }
                ShowSnackbar(errors, Severity.Error);

            }
            catch (Exception ex)
            {

                ShowSnackbar(ex.Message, Severity.Error);

            }

        }

        /// <summary>
        /// Displays a snackbar notification with the specified message and severity.
        /// </summary>
        /// <param name="message">The message to display in the snackbar.</param>
        /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
        void ShowSnackbar(string message, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
        }

        public void CancelEdit()
        {
            Navigation.NavigateTo("/users", true);

        }
    



}
