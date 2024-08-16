import clsx from "clsx";
import { FC, ReactNode } from "react";

interface ButtonProps {
  children: ReactNode;
  className?: string;
  onClick: () => void;
}

export const Button: FC<ButtonProps> = ({ children, className, onClick }) => {
  return (
    <button
      onClick={onClick}
      className={clsx(
        "mt-8 flex w-[80%] items-center justify-center rounded-lg border border-gray-300 py-2 text-lg text-gray-600 duration-300 ease-in-out hover:bg-slate-100",
        className
      )}
    >
      {children}
    </button>
  );
};
