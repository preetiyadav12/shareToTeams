// © Microsoft Corporation. All rights reserved.
import React from 'react';
import { useMsal } from '@azure/msal-react';
import { loginRequest } from '../authConfig';
import DropdownButton from 'react-bootstrap/DropdownButton';
import Dropdown from 'react-bootstrap/esm/Dropdown';

export const SignInButton = (): JSX.Element => {

    const { instance } = useMsal();

    const handleLogin = (loginType: string) => {
      if (loginType === 'popup') {
        instance.loginPopup(loginRequest).catch((e) => {
          console.log(e);
        });
      } else if (loginType === 'redirect') {
        instance.loginRedirect(loginRequest).catch((e) => {
          console.log(e);
        });
      }
    };
    return (
      <DropdownButton variant="secondary" className="ml-auto" drop="left" title="Sign In">
        <Dropdown.Item as="button" onClick={() => handleLogin('popup')}>
          Sign in using Popup
        </Dropdown.Item>
        <Dropdown.Item as="button" onClick={() => handleLogin('redirect')}>
          Sign in using Redirect
        </Dropdown.Item>
      </DropdownButton>
    );

  
};
