import EntityAPI from '../src/api/entityMapping';
import { AppSettings } from '../src/config/appSettings';
import { EntityState } from '../src/models';

jest.mock('../src/api/entity');

const EntityAPIMock = EntityAPI.getMapping as jest.MockedFunction<
  typeof EntityAPI.getMapping
>;

EntityAPIMock.mockImplementation(async (entityId) => {
  const initResponse: EntityState = {
    entityId: '0123',
    acsToken: 'fakedToken',
    acsUserId: '0123',
    threadId: 'fakeId',
  };
  return initResponse;
});

describe('Entity Mapping API Test Handler', () => {
  beforeAll(() => {
    EntityAPIMock.mockClear();
  });

  it('throws error if ACS Token is emtpy', async () => {
    const config: AppSettings = {
      hostDomain: '',
      apiBaseUrl: '',
      clientId: '',
      tenant: '',
      acsEndpoint: '',
    };
    const response = await EntityAPI.getMapping('0001', '', config);

    expect(EntityAPIMock).toBeCalled();
    expect(response?.acsToken).toBe('fakedToken');
  });
});
