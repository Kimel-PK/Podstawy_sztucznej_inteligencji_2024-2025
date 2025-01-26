import torch
import torchvision
import torchvision.transforms as transforms
import matplotlib.pyplot as plt
import numpy as np
import torch.nn as nn
import torch.nn.functional as F

transform = transforms.Compose([transforms.ToTensor(), transforms.Normalize((0.5, 0.5, 0.5), (0.5, 0.5, 0.5))])

batch_size = 4

testset = torchvision.datasets.CIFAR10(root='./data', train=False, download=True, transform=transform)
testloader = torch.utils.data.DataLoader(testset, batch_size=batch_size, shuffle=False, num_workers=2)

classes = ('plane', 'car', 'bird', 'cat', 'deer', 'dog', 'frog', 'horse', 'ship', 'truck')

# functions to show an image

def imshow(img):
    img = img / 2 + 0.5     # unnormalize
    npimg = img.numpy()
    plt.imshow(np.transpose(npimg, (1, 2, 0)))
    plt.show()

class Net(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(3, 6, 5)
        self.pool = nn.MaxPool2d(2, 2)
        self.conv2 = nn.Conv2d(6, 16, 5)
        self.fc1 = nn.Linear(16 * 5 * 5, 120)
        self.fc2 = nn.Linear(120, 84)
        self.fc3 = nn.Linear(84, 10)

    def forward(self, x):
        x = self.pool(F.relu(self.conv1(x)))
        x = self.pool(F.relu(self.conv2(x)))
        x = torch.flatten(x, 1) # flatten all dimensions except batch
        x = F.relu(self.fc1(x))
        x = F.relu(self.fc2(x))
        x = self.fc3(x)
        return x

if __name__ == '__main__':

	# load classifier
	net = Net()
	net.load_state_dict(torch.load('./cifar_net.pth', weights_only=True))
    
	# show accuracy of whole set
	correct = 0
	total = 0
      
	# prepare to count predictions for each class
	correct_pred = {classname: 0 for classname in classes}
	total_pred = {classname: 0 for classname in classes}
	
	# since we're not training, we don't need to calculate the gradients for our outputs
	with torch.no_grad():
		for data in testloader:
			images, labels = data
			# calculate outputs by running images through the network
			outputs = net(images)
			# the class with the highest energy is what we choose as prediction
			_, predictions = torch.max(outputs, 1)
			total += labels.size(0)
			correct += (predictions == labels).sum().item()
            
        	# collect the correct predictions for each class
			for label, prediction in zip(labels, predictions):
				if label == prediction:
					correct_pred[classes[label]] += 1
				total_pred[classes[label]] += 1

	# whole accuracy
	print(f'Accuracy of the network on the 10000 test images: {100 * correct // total} %')
	
	# print accuracy for each class
	for classname, correct_count in correct_pred.items():
		accuracy = 100 * float(correct_count) / total_pred[classname]
		print(f'Accuracy for class: {classname:5s} is {accuracy:.1f} %')

	# show accuracy of random 4 images
	
	dataiter = iter(testloader)
	images, labels = next(dataiter)

	print('GroundTruth: ', ' '.join(f'{classes[labels[j]]:5s}' for j in range(4)))
    
	outputs = net(images)
    
	_, predicted = torch.max(outputs, 1)

	print('Predicted: ', ' '.join(f'{classes[predicted[j]]:5s}' for j in range(4)))
	
	# print images
	imshow(torchvision.utils.make_grid(images))