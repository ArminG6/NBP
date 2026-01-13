import { ButtonHTMLAttributes, ReactNode } from 'react';
import { LoadingSpinner } from './LoadingSpinner';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  children: ReactNode;
}

export function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  leftIcon,
  rightIcon,
  children,
  disabled,
  className = '',
  ...props
}: ButtonProps) {
  const baseClasses = `inline-flex items-center justify-center font-medium
                       transition-all duration-200 ease-out rounded-xl
                       focus:outline-none focus:ring-2 focus:ring-offset-2 
                       focus:ring-offset-dark-950 disabled:opacity-50 
                       disabled:cursor-not-allowed`;

  const variantClasses = {
    primary: `bg-gradient-to-r from-primary-500 to-accent-500 text-white
              hover:from-primary-400 hover:to-accent-400 focus:ring-primary-500
              shadow-lg shadow-primary-500/25 hover:shadow-xl hover:shadow-primary-500/30`,
    secondary: `bg-dark-800 text-dark-100 border border-dark-600
                hover:bg-dark-700 hover:border-dark-500 focus:ring-dark-500`,
    danger: `bg-red-600 text-white hover:bg-red-500 focus:ring-red-500
             shadow-lg shadow-red-500/25`,
    ghost: `bg-transparent text-dark-300 hover:bg-dark-800 hover:text-dark-100
            focus:ring-dark-500`,
  };

  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm gap-1.5',
    md: 'px-4 py-2.5 text-base gap-2',
    lg: 'px-6 py-3 text-lg gap-2.5',
  };

  return (
    <button
      className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <LoadingSpinner size="sm" />
      ) : (
        <>
          {leftIcon && <span className="flex-shrink-0">{leftIcon}</span>}
          {children}
          {rightIcon && <span className="flex-shrink-0">{rightIcon}</span>}
        </>
      )}
    </button>
  );
}
