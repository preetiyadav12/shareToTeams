
import React from 'react';
import { useMsal } from '@azure/msal-react';
import DropdownButton from 'react-bootstrap/DropdownButton';
import Dropdown from 'react-bootstrap/esm/Dropdown';

export const SignOutButton = (): JSX.Element => {

    const { instance } = useMsal();

  const handleLogout = (logoutType: string) => {
    if (logoutType === 'popup') {
      instance.logoutPopup({
        postLogoutRedirectUri: '/',
        mainWindowRedirectUri: '/'
      });
    } else if (logoutType === 'redirect') {
      instance.logoutRedirect({
        postLogoutRedirectUri: '/'
      });
    }
  };
  return (
    <DropdownButton variant="secondary" className="ml-auto" drop="left" title="Sign Out">
      <Dropdown.Item as="button" onClick={() => handleLogout('popup')}>
        Sign out using Popup
      </Dropdown.Item>
      <Dropdown.Item as="button" onClick={() => handleLogout('redirect')}>
        Sign out using Redirect
      </Dropdown.Item>
    </DropdownButton>
  );
  
};
