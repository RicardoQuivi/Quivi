import { useState } from "react";
import { Dropdown } from "../ui/dropdown/Dropdown";
import { Link } from "react-router";
import { useAuth, useAuthenticatedUser } from "../../context/AuthContext";
import { ChevronDownIcon, SignOutIcon } from "../../icons";
import Avatar from "../ui/avatar/Avatar";
import { useTranslation } from "react-i18next";

export const UserDropdown = () => {
    const { t } = useTranslation();
    const auth = useAuth();
    const user = useAuthenticatedUser();
    
    const [isOpen, setIsOpen] = useState(false);
    const closeDropdown = () => setIsOpen(false);

    const getName = () => {
        const name = user.name.split(" ");
        if(name.length > 1) {
            return name[0];
        }
        const name2 = user.name.split("@");
        if(name2.length > 1) {
            return name2[0].split(".")[0].split("-")[0].split("_")[0];
        }
        return user.name;
    }

    return (
        <div className="relative">
            <button
                onClick={() => setIsOpen(s => !s)}
                className="flex items-center text-gray-700 dropdown-toggle dark:text-gray-400"
            >
                <div
                    className="mr-3"
                >
                    <Avatar
                        size="medium" 
                        alt={user.name}
                    />
                </div>

                <span className="block mr-1 font-medium text-theme-sm first-letter:uppercase">{getName()}</span>
                <ChevronDownIcon
                    className={`stroke-gray-500 dark:stroke-gray-400 transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
                />
            </button>

            <Dropdown
                isOpen={isOpen}
                onClose={closeDropdown}
                className="absolute right-0 mt-[17px] flex w-[260px] flex-col rounded-2xl border border-gray-200 bg-white p-3 shadow-theme-lg dark:border-gray-800 dark:bg-gray-dark"
            >
                <div>
                    <span className="block font-medium text-gray-700 text-theme-sm dark:text-gray-400">
                        {user.name}
                    </span>
                    <span className="mt-0.5 block text-theme-xs text-gray-500 dark:text-gray-400">
                        {user.email}
                    </span>
                </div>

                <ul className="flex flex-col gap-1 pt-4 pb-3 border-b border-gray-200 dark:border-gray-800">
                    {/* <li>
                        <DropdownItem
                            onItemClick={closeDropdown}
                            tag="a"
                            to="/profile"
                            className="flex items-center gap-3 px-3 py-2 font-medium text-gray-700 rounded-lg group text-theme-sm hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-gray-300"
                        >
                            <ProfileIcon
                                className="fill-gray-500 group-hover:fill-gray-700 dark:fill-gray-400 dark:group-hover:fill-gray-300 size-[24px]"
                            />
                            {t("appHeader.editProfile")}
                        </DropdownItem>
                    </li>
                    <li>
                        <DropdownItem
                            onItemClick={closeDropdown}
                            tag="a"
                            to="/profile"
                            className="flex items-center gap-3 px-3 py-2 font-medium text-gray-700 rounded-lg group text-theme-sm hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-gray-300"
                        >
                            <GearIcon
                                className="fill-gray-500 group-hover:fill-gray-700 dark:fill-gray-400 dark:group-hover:fill-gray-300 size-[24px]"
                            />
                            {t("appHeader.accountSettings")}
                        </DropdownItem>
                    </li> 
                    <li>
                        <DropdownItem
                            onItemClick={closeDropdown}
                            tag="a"
                            to="/profile"
                            className="flex items-center gap-3 px-3 py-2 font-medium text-gray-700 rounded-lg group text-theme-sm hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-gray-300"
                        >
                            <InfoIcon
                                className="fill-gray-500 group-hover:fill-gray-700 dark:fill-gray-400 dark:group-hover:fill-gray-300 size-[24px]"
                            />
                            {t("appHeader.support")}
                        </DropdownItem>
                    </li> */}
                </ul>
                <Link
                    to="/signin"
                    className="flex items-center gap-3 px-3 py-2 mt-3 font-medium text-gray-700 rounded-lg group text-theme-sm hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-gray-300"
                    onClick={auth.signOut}
                >
                    <SignOutIcon
                        className="fill-gray-500 group-hover:fill-gray-700 dark:group-hover:fill-gray-300 size-[24px]"
                    />
                    {t("appHeader.signOut")}
                </Link>
            </Dropdown>
        </div>
    );
}
