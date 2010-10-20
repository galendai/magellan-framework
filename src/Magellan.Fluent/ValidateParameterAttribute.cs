﻿using System;
using FluentValidation;
using Magellan.Framework;

namespace Magellan.Fluent
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ValidateParameterAttribute : Attribute, IActionFilter
    {
        private readonly string _routeName;
        private readonly Type _validatorType;

        public ValidateParameterAttribute(string routeName, Type validatorType)
        {
            _routeName = routeName;
            _validatorType = validatorType;
        }

        public Type ValidatorType
        {
            get { return _validatorType; }
        }

        public string RouteName
        {
            get { return _routeName; }
        }

        protected virtual IValidator CreateValidator()
        {
            return (IValidator) Activator.CreateInstance(ValidatorType);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var validator = CreateValidator();

            var model = context.Request.RouteValues[RouteName];
            var result = validator.Validate(model);

            var store = model as IStoreValidationMessages;
            if (store != null)
            {
                store.ValidationMessages.Clear();
                foreach (var error in result.Errors)
                {
                    store.ValidationMessages.Add(error.PropertyName, error.ErrorMessage);
                }
            }

            if (!result.IsValid)
            {
                var flasher = model as IFlasher;
                if (flasher != null)
                {
                    flasher.ClearFlashes();
                    flasher.Flash("Please ensure all fields have been entered correctly.");
                }

                context.OverrideResult = new DoNothingResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
