import { PrimaryButton, Stack, Spinner } from '@fluentui/react';
import { ChatIcon } from '@fluentui/react-icons-northstar';
import React, { useEffect, useState } from 'react';
import { FocusZone, FocusZoneDirection } from 'office-ui-fabric-react/lib/FocusZone';

import { buttonStyle, chatIconStyle, mainContainerStyle } from './styles/ConfigurationScreen.styles';
import {
  labelFontStyle,
  largeAvatarContainerStyle,
  largeAvatarStyle,
  leftPreviewContainerStyle,
  namePreviewStyle,
  responsiveLayoutStyle,
  rightInputContainerStyle,
  smallAvatarContainerStyle,
  smallAvatarStyle,
  startChatButtonTextStyle
} from './styles/ConfigurationScreen.styles';
import { CAT, MOUSE, KOALA, OCTOPUS, MONKEY, FOX, getThreadId } from '../utils/utils';

import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from '@azure/msal-react';
import { loginRequest, msalConfig } from '../authConfig';
import { CreateOrGetACSUser, GetAcsToken } from '../acsAuthApiCaller';
import { PageLayout } from './PageLayout';
import { PublicClientApplication, SilentRefreshClient } from '@azure/msal-browser';
import { SignInButton } from './SignInButton';
//import { RequestuserData } from '../containers/ConfigurationScreen';


export interface ConfigurationScreenProps {
  joinChatHandler(): void;
  setup(displayName: string | null, emoji: string, acsToken: any, token: any, acsID: string): void;
  isValidThread(threadId: string | null): any;
}

export default (props: ConfigurationScreenProps): JSX.Element => {
  const { instance, accounts } = useMsal();
  const [acsToken, setAcsToken] = useState('');
  const [token, setToken] = useState('');
  const [acsID, setId] = useState('');


  const spinnerLabel = 'Initializing chat client...';

  const avatarsList = [CAT, MOUSE, KOALA, OCTOPUS, MONKEY, FOX];
  const [name, setName] = useState<string | null>(null);//useState();
  const [selectedAvatar, setSelectedAvatar] = useState(CAT);

  const [isJoining, setIsJoining] = useState(false);

  const [isValidThread, setIsValidThread] = useState<boolean | undefined>(undefined);

  const { joinChatHandler, setup } = props;

  const onAvatarChange = (newAvatar: string) => {
    setSelectedAvatar(newAvatar);
  };


  const isValidThreadProp = props.isValidThread;

  const isAuthenticated = useIsAuthenticated();

  const RequestuserData = async () => {

    try {
      instance
        .acquireTokenSilent({
          ...loginRequest,
          account: accounts[0]
        })
        .then((response) => {
          CreateOrGetACSUser(response.accessToken)
            .then(() => {
              setName(response.account!.username);
              GetAcsToken(response.accessToken)
                .then((message) => {
                  setAcsToken(message.token);
                  setId(message.user.id);
                });
            })
            .catch((error) => console.log(error));

        });
      // Silently acquires an access token
      instance
        .acquireTokenSilent({
          ...loginRequest,
          account: accounts[0]
        })
        .then((response) => {
          setToken(response.accessToken);
        });

    }
    catch (error) {
      console.log(error);
    }
    return name;
  }


  useEffect(() => {

    const isValidThread = async () => {
      if (await isValidThreadProp(getThreadId())) {
        setIsValidThread(true);
      } else {
        setIsValidThread(false);
      }
    };
    isValidThread();
    document.getElementById('🐱')?.focus();

  }, [isValidThreadProp]);

  const invalidChatThread = () => {
    return (
      <div>
        <p>thread Id is not valid</p>
      </div>
    );
  };

  const joinChatLoading = () => {
    return <Spinner label={spinnerLabel} ariaLive="assertive" labelPosition="top" />;
  };



  const joinChatArea = () => {
    if (isAuthenticated) {
      RequestuserData();
    }

    return (
      <div>

        <Stack className={responsiveLayoutStyle} horizontal={true} horizontalAlign="center" verticalAlign="center">
          <Stack
            className={leftPreviewContainerStyle}
            horizontal={false}
            verticalAlign="center"
            horizontalAlign="center"
            tokens={{ childrenGap: 13 }}
          >
            <div className={largeAvatarContainerStyle(selectedAvatar)}>
              <div className={largeAvatarStyle}>{selectedAvatar}</div>
            </div>
            <div aria-label="Display name" className={namePreviewStyle(name !== '')}>
              {name !== '' ? name : 'Name'}
            </div>
          </Stack>
          <Stack className={rightInputContainerStyle} horizontal={false} tokens={{ childrenGap: 20 }}>
            <div>
              <div className={labelFontStyle}>Avatar</div>
              <FocusZone direction={FocusZoneDirection.horizontal}>
                <Stack role="list" horizontal={true} tokens={{ childrenGap: 4 }}>
                  {avatarsList.map((avatar, index) => (
                    <div
                      role="listitem"
                      id={avatar}
                      key={index}
                      tabIndex={0}
                      data-is-focusable={true}
                      className={smallAvatarContainerStyle(avatar, selectedAvatar)}
                      onFocus={() => onAvatarChange(avatar)}
                    >
                      <div className={smallAvatarStyle}>{avatar}</div>
                    </div>
                  ))}
                </Stack>
              </FocusZone>
            </div>
            Hello {name}
            <div>
              <PrimaryButton
                id="join"
                className={buttonStyle}
                disabled={name?.length == 0 || name == null ? true : false}
                onClick={() => {
                  setup(name, selectedAvatar, acsToken, token, acsID);

                  joinChatHandler();

                }}
              >

                <ChatIcon className={chatIconStyle} size="medium" />
                <div className={startChatButtonTextStyle}>Join chat</div>
              </PrimaryButton>
            </div>
          </Stack>
        </Stack>
      </div>
    );
  };

  const MainContent = () => {
    return (
      <div className="App">
        <AuthenticatedTemplate>
          <Stack className={mainContainerStyle} horizontalAlign="center" verticalAlign="center">

            {isValidThread === false ? invalidChatThread() : joinChatArea()

            }
          </Stack>
        </AuthenticatedTemplate>

        <UnauthenticatedTemplate>
          <h5 className="card-title">Please sign-in to join the chat.</h5>
        </UnauthenticatedTemplate>
      </div>
    );
  };

  const configurationScreen = () => {
    return (
      <PageLayout>

        <MainContent />
      </PageLayout>

    );
  };

  return isJoining ? joinChatLoading() : configurationScreen();
};
